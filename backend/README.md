# STRIDE Backend (ASP.NET Core)

This repo contains the ASP.NET Core API for:

- Auth: register / login / me
- Purchases: create + list purchases for the signed-in user

Data is stored locally (dev) in `App_Data/stride-db.json` (created at runtime).

## Run locally

```powershell
dotnet restore
dotnet run --urls http://localhost:5231
```

Open:

- `http://localhost:5231/` (serves the included `wwwroot` storefront)
- API:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/me` (Bearer token)
  - `POST /api/purchases` (Bearer token)
  - `GET /api/purchases` (Bearer token)

## Config

JWT settings are in `appsettings.json` under `Jwt`. For real deployments, replace the signing key using environment variables / secrets.

## Deploy on Render (Docker)

Render doesn’t have a dedicated ASP.NET preset in the UI. Deploy this repo as a **Docker** web service using the included `Dockerfile`.

Minimum env vars to set on Render:

- `Jwt__SigningKey` (replace the dev key)
- Optional (recommended): `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`

If you want purchases/users to persist across deploys, attach a Render disk and set:

- `STRIDE_DATA_DIR=/var/data` (or whatever mount path you choose)
