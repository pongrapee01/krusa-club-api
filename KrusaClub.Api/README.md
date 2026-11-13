# KrusaClub.Api (.NET 8 + Supabase)

## What is prepared

- JWT authentication with `JwtBearer` against Supabase JWT secret
- Extract user identifier from `sub` claim via `ICurrentUserAccessor`
- Direct PostgreSQL access through `Npgsql` + `Dapper`
- Example secured endpoints:
  - `GET /api/me/id`
  - `GET /api/me/profile`

## Configure Supabase values

Edit `appsettings.json` (or use environment variables):

- `Supabase:ConnectionString`
- `Supabase:JwtSecret`
- `Supabase:JwtIssuer` (default `supabase`)
- `Supabase:JwtAudience` (default `authenticated`)

## Run

```bash
dotnet run --project KrusaClub.Api
```

## React call example

```ts
const token = session.access_token; // from Supabase auth session

const response = await fetch("https://localhost:5001/api/me/id", {
  headers: {
    Authorization: `Bearer ${token}`,
  },
});
```

## Notes

- `UserProfileRepository` currently reads from `public.profiles` by `id = sub`.
- Adjust SQL/table name to match your actual Supabase schema.
