FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Stride.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

# Render (and many PaaS) provide PORT at runtime.
EXPOSE 10000
ENV ASPNETCORE_ENVIRONMENT=Production

CMD ["sh", "-c", "dotnet Stride.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]

