using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Stride.Api.Data;
using Stride.Api.Services;
using Stride.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var databaseUrl = builder.Configuration["DATABASE_URL"];
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        connectionString = new NpgsqlConnectionStringBuilder(databaseUrl)
        {
            SslMode = SslMode.Require
        }.ToString();
    }
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing database connection string. Set ConnectionStrings:DefaultConnection or DATABASE_URL.");
}

// Forwarded headers for Render / proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Configuration
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException(
        "Missing JWT signing key in configuration.");
}

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<TokenService>();

// Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PurchaseRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        policy
            .WithOrigins(
                "https://stride-frontend-one.vercel.app",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();
// HTTPS
//app.UseHttpsRedirection();

// CORS
app.UseCors("AllowVercel");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Explicit guidance for auth route methods
app.MapMethods("/api/auth/register", new[] { "GET", "OPTIONS" },
    () => Results.Json(new
    {
        message = "Use POST /api/auth/register with JSON body { username, email, password }."
    }, statusCode: 405));

app.MapMethods("/api/auth/login", new[] { "GET", "OPTIONS" },
    () => Results.Json(new
    {
        message = "Use POST /api/auth/login with JSON body { identifier, password }."
    }, statusCode: 405));

// Controllers
app.MapControllers();

// Health Check Route
app.MapMethods("/", new[] { "GET", "HEAD", "OPTIONS" },
    () => Results.Text("Stride API is running successfully."));

app.MapGet("/api", () => Results.Json(new
{
    endpoints = new[]
    {
        "POST /api/auth/register",
        "POST /api/auth/login",
        "GET /api/auth/me",
        "GET /api/purchases",
        "POST /api/purchases"
    }
}));

app.MapGet("/error", () => Results.Problem("An unexpected server error occurred."));

app.MapFallback(() => Results.Problem("Endpoint not found.", statusCode: 404));

app.Run();