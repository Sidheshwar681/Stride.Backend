using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using System.Collections.Generic;
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stride API",
        Version = "v1"
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token as: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    // Swashbuckle with Microsoft.OpenApi expects a factory that receives the
    // generated OpenApiDocument and returns an OpenApiSecurityRequirement.
    options.AddSecurityRequirement(doc =>
    {
        var req = new OpenApiSecurityRequirement();
        // Create a reference to the security scheme by id using the document.
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc, null);
        req.Add(schemeRef, new List<string>());
        return req;
    });
});

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
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "JWT authentication failed.");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "JWT bearer challenge: {Error} {ErrorDescription}",
                    context.Error,
                    context.ErrorDescription);
                return Task.CompletedTask;
            }
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
    // Allow passing a frontend origin via config or env var (useful on Render)
    var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? Environment.GetEnvironmentVariable("FRONTEND_ORIGIN");

    options.AddPolicy("AllowVercel", policy =>
    {
        var origins = new List<string>
        {
            "https://stride-frontend-one.vercel.app",
            "http://localhost:5173"
        };

        if (!string.IsNullOrWhiteSpace(frontendOrigin))
        {
            origins.Add(frontendOrigin);
        }

        policy
            .WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// If a hosting platform (like Render) provides a PORT env var, bind to it on 0.0.0.0
var portEnv = Environment.GetEnvironmentVariable("PORT") ?? builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out var p))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{p}");
}

var app = builder.Build();
// Database Startup Check
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("=== DATABASE CONNECTIVITY TEST ===");

        var canConnect = await db.Database.CanConnectAsync();

        Console.WriteLine($"Database connected: {canConnect}");

        if (canConnect)
        {
            Console.WriteLine("Database connection successful.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("DATABASE ERROR:");
        Console.WriteLine(ex.ToString());
    }
}
// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }
// else
// {
//     app.UseExceptionHandler("/error");
// }

app.UseForwardedHeaders();
// HTTPS
//app.UseHttpsRedirection();

// CORS
app.UseCors("AllowVercel");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

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

app.Run();