# Zust — Social Media App

Zust is a full-featured social media web application built with **ASP.NET Core MVC**. Users can create profiles, share posts (text, images and video), like and comment, send and accept friend requests, chat in real time, and receive live notifications.

> Originally built on .NET 6 + SQL Server, Zust has been modernized to run on **.NET 10** with **PostgreSQL (Supabase)** and is deployable to **Render** via Docker.

[![Showcase video](https://media.aykhan.net/thumbnails/step-it-academy/asp-net/task27.png)](https://www.youtube.com/watch?v=vOdeSuh_zMY)

*▶️ [Watch the showcase video](https://www.youtube.com/watch?v=vOdeSuh_zMY)*

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Local Development](#local-development)
- [Supabase Setup](#supabase-setup)
- [Render Deployment](#render-deployment)
- [Environment Variables](#environment-variables)
- [Demo Data](#demo-data)
- [Screenshots](#screenshots)
- [License](#license)

---

## Features

- 🔐 **Authentication & profiles** — registration, login and rich user profiles (ASP.NET Core Identity).
- 📝 **Posts** — create text/image/video posts with media hosted on Cloudinary.
- ❤️ **Likes & comments** — engage with posts.
- 👥 **Friendships** — send, accept and manage friend requests.
- 💬 **Real-time chat** — one-to-one messaging powered by SignalR.
- 🔔 **Live notifications** — instant updates via SignalR.
- 📰 **News feed** — posts, videos and suggestions.

## Tech Stack

| Layer        | Technology                                            |
|--------------|-------------------------------------------------------|
| Runtime      | .NET 10 (ASP.NET Core MVC + Razor views)              |
| Real-time    | SignalR                                               |
| Auth         | ASP.NET Core Identity (cookie-based)                  |
| Data         | Entity Framework Core 10 (Code First)                 |
| Database     | PostgreSQL (Supabase)                                 |
| Media        | Cloudinary                                            |
| Mapping      | AutoMapper                                            |
| Hosting      | Render (Docker)                                       |

## Architecture

Zust is a single, server-rendered ASP.NET Core application organized in layers (no separate frontend — the UI is rendered with Razor, so there is **no need for a Vercel/Next.js frontend**):

```
Zust.sln
├── Zust.Web          → ASP.NET Core MVC app: controllers, Razor views, SignalR hubs, DI/startup (Program.cs)
├── Zust.Business     → application/service layer (business logic)
├── Zust.DataAccess   → EF Core DbContext, repositories, migrations helpers, seeding
├── Zust.Core         → cross-cutting abstractions
└── Zust.Entities     → domain models (User, Post, Comment, Like, Friendship, Chat, Message, Notification, …)
```

**Request flow:** Browser → MVC controllers / API controllers → Business services → DataAccess (EF Core) → PostgreSQL. Real-time features go Browser ⇄ SignalR `UserHub`.

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ running locally (or a Supabase connection string — see below)
- (Optional) A [Cloudinary](https://cloudinary.com/) account for media uploads

### Steps

```bash
# 1. Clone
git clone https://github.com/aykhan019/zust
cd zust

# 2. Configure local secrets (kept out of git)
cp Zust/appsettings.Development.json.example Zust/appsettings.Development.json
#   then edit Zust/appsettings.Development.json with your local Postgres + Cloudinary values

# 3. Restore & build
dotnet restore
dotnet build

# 4. Apply migrations to your local database
dotnet ef database update --project Zust/Zust.Web.csproj --startup-project Zust/Zust.Web.csproj

# 5. Run
dotnet run --project Zust/Zust.Web.csproj
```

The app reads its connection string from `ConnectionStrings:Default` (or a `DATABASE_URL` URI). In Development this comes from `appsettings.Development.json`; in Production from environment variables.

> **Tip:** Prefer not to keep a connection string in a file? Use user secrets instead:
> ```bash
> dotnet user-secrets init --project Zust/Zust.Web.csproj
> dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=zust;Username=postgres;Password=postgres" --project Zust/Zust.Web.csproj
> ```

## Supabase Setup

1. Create a project at [supabase.com](https://supabase.com) and choose a strong database password.
2. In the dashboard go to **Project Settings → Database → Connection string** and copy the connection details. You have two options:
   - **Direct connection** (`db.<ref>.supabase.co:5432`) — best for migrations and a persistent backend, **if your host has IPv6**.
   - **Session pooler** (`aws-0-<region>.pooler.supabase.com:5432`, user `postgres.<ref>`) — use this when your host is **IPv4-only**. **Render's free tier is IPv4, so use the session pooler.**
3. Build an Npgsql connection string (SSL is required by Supabase):

   ```
   Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true
   ```

   This is the value you set as `ConnectionStrings__Default`. (Alternatively, Zust also accepts a `DATABASE_URL` in `postgresql://user:pass@host:5432/postgres` URI form and converts it automatically, forcing SSL.)
4. Apply migrations against Supabase:

   ```bash
   ConnectionStrings__Default="Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true" \
     dotnet ef database update --project Zust/Zust.Web.csproj --startup-project Zust/Zust.Web.csproj
   ```

   > Migrations are also applied automatically on app startup, so this step is optional if you deploy straight to Render.

## Render Deployment

Zust deploys to Render as a **Docker web service**. A [`render.yaml`](./render.yaml) blueprint is included.

**Option A — Blueprint (recommended):**
1. Push this repo to GitHub.
2. In Render, **New → Blueprint**, point it at the repo. Render reads `render.yaml`.
3. Fill in the environment variables marked `sync: false` (see below) in the dashboard.
4. Deploy. Render builds the `Dockerfile`, injects `PORT`, and the app binds Kestrel to it.

**Option B — Manual:**
1. **New → Web Service**, connect the repo, choose **Docker** runtime.
2. Set **Health Check Path** to `/home/index`.
3. Add the environment variables below.

The app listens on Render's `PORT`, honors `X-Forwarded-*` headers (correct HTTPS/cookies behind Render's proxy), serves static files and runs SignalR over the same origin. EF Core migrations run automatically on startup.

## Environment Variables

| Variable                          | Required | Description                                                                 |
|-----------------------------------|:--------:|-----------------------------------------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`          | ✅       | `Production` on Render, `Development` locally.                               |
| `ConnectionStrings__Default`      | ✅       | Npgsql connection string for Supabase PostgreSQL (see Supabase Setup).       |
| `AppSettings__Token`              | ✅       | App token/secret used for token signing. Use a long random value.            |
| `CloudinarySettings__CloudName`   | ✅*      | Cloudinary cloud name (*required for media uploads).                          |
| `CloudinarySettings__ApiKey`      | ✅*      | Cloudinary API key.                                                          |
| `CloudinarySettings__ApiSecret`   | ✅*      | Cloudinary API secret.                                                       |
| `DATABASE_URL`                    | ⬜       | Alternative to `ConnectionStrings__Default`; a `postgresql://…` URI.          |
| `ALLOWED_ORIGINS`                 | ⬜       | Comma-separated extra CORS origins (only needed if you add an external SPA). |
| `SEED_DEMO_DATA`                  | ⬜       | Set to `true` once to seed demo users/posts, then remove.                    |

> Locally these live in `Zust/appsettings.Development.json` (git-ignored). Never commit real secrets — `appsettings.json` ships with empty placeholders only.

## Demo Data

To present the app publicly with content, set `SEED_DEMO_DATA=true` for one run. On startup, after migrations, Zust seeds a handful of demo users and posts (idempotent — it only inserts when the tables are empty).

Demo accounts use the password **`Demo@1234`** (e.g. `aladdin@zust.demo`, `maya@zust.demo`, `liam@zust.demo`, `sofia@zust.demo`). Remove `SEED_DEMO_DATA` after the first successful run.

## Screenshots

> _Add screenshots / a short demo GIF here for your portfolio._
>
> | Feed | Profile | Chat |
> |------|---------|------|
> | _(screenshot)_ | _(screenshot)_ | _(screenshot)_ |

## License

Zust is released under the [MIT License](LICENSE).
