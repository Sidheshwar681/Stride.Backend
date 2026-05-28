# STRIDE Frontend (Vercel)

Deploy this repo to Vercel as a static site.

## Deploy

1. Push to GitHub (already done).
2. In Vercel: **New Project** → import this repository.
3. Framework preset: **Other**.

## Connect to the ASP.NET API

The frontend calls:

- `/api/auth/*`
- `/api/purchases`

After you deploy your ASP.NET Core backend somewhere, add a rewrite in `vercel.json`:

```json
{
  "rewrites": [
    {
      "source": "/api/:path*",
      "destination": "https://YOUR_BACKEND_DOMAIN/api/:path*"
    }
  ]
}
```

