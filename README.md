# PostHog Sample .NET Web API

A minimal ASP.NET Core Web API that demonstrates [PostHog](https://posthog.com) integration for:

- **Events & properties** — capture, identify, alias
- **Actions & cohorts** — read/create via PostHog private REST API
- **Dashboards & insights** — list, create, and run dashboard insights
- **Feature flags & experiments** — evaluate flags and A/B variants
- **Error tracking** — manual `$exception` capture

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer (.NET 9 also works via `global.json` roll-forward)
- A PostHog Cloud or self-hosted project

### Visual Studio (Windows)

Use **Visual Studio 2022 17.8 or later** and install these workloads via **Visual Studio Installer → Modify**:

1. **ASP.NET and web development** (required — without this you get *"This setup for this installation of Visual Studio is not complete"*)
2. **.NET desktop development** (recommended)

Then confirm the **.NET 8.0 Runtime** and **.NET 8.0 SDK** individual components are checked.

If the installer shows a **Resume** or **Repair** button, run that first before opening the solution.

### "Access denied" on `.exe` (Windows)

Cloned repos are often marked as downloaded from the internet. Windows Defender or SmartScreen may block `PostHogSample.Api.exe` in `bin\`.

**Recommended — run via `dotnet` (no `.exe` needed):**

```powershell
dotnet run --project src/PostHogSample.Api
```

Or double-click `run.cmd` in the repo root.

**If it still fails, unblock the repo once (PowerShell as your user):**

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\unblock-windows.ps1
```

Then clean and rebuild:

```powershell
dotnet clean PostHogSample.sln
dotnet build PostHogSample.sln
dotnet run --project src/PostHogSample.Api
```

Also check:

- Clone to a local folder (e.g. `C:\dev\sample-webapi`), not a network/sync path that restricts executables
- Temporarily allow the project `bin` folder in antivirus if it quarantines build output
- Close any running instance of the API before rebuilding (a locked `.exe` shows as "denied")

### `dotnet.exe` exited with code `-532462766` (0xE0434352)

This means the app **crashed on startup** with an unhandled .NET exception. Common causes on Windows:

**1. HTTPS launch profile without a dev certificate (most common)**

Use the **http** launch profile, or run from the command line:

```powershell
dotnet run --project src/PostHogSample.Api --launch-profile http
```

If you need HTTPS locally:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

**2. Missing .NET runtime**

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (includes ASP.NET Core runtime). The project targets `net8.0` and `net9.0`.

Check installed runtimes:

```powershell
dotnet --list-runtimes
dotnet --list-sdks
```

**3. See the real error message**

Run from a terminal so the exception is printed:

```powershell
dotnet run --project src/PostHogSample.Api --launch-profile http
```

Or in Visual Studio: **View → Output**, show output from **Debug**.

You can also open the project without Visual Studio:

```bash
dotnet restore PostHogSample.sln
dotnet build PostHogSample.sln
dotnet run --project src/PostHogSample.Api
```

## Quick start

```bash
git clone <repo-url>
cd sample-webapi
dotnet restore PostHogSample.sln
dotnet run --project src/PostHogSample.Api
```

The API listens on `http://localhost:5032` by default (see `src/PostHogSample.Api/Properties/launchSettings.json`).

Open `http://localhost:5032/api/health` to confirm the service is running.

## Configuration

Edit `src/PostHogSample.Api/appsettings.json` or use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd src/PostHogSample.Api
dotnet user-secrets init
dotnet user-secrets set "PostHog:ProjectToken" "phc_your_project_token"
dotnet user-secrets set "PostHog:HostUrl" "https://us.i.posthog.com"
dotnet user-secrets set "PostHog:PersonalApiKey" "phx_your_personal_api_key"
dotnet user-secrets set "PostHogAdminApi:ProjectId" "12345"
dotnet user-secrets set "PostHogAdminApi:AppHostUrl" "https://us.posthog.com"
```

| Setting | Purpose |
|--------|---------|
| `PostHog:ProjectToken` | Required for event capture, feature flags, and error tracking |
| `PostHog:HostUrl` | Ingestion host (`https://us.i.posthog.com` or `https://eu.i.posthog.com`) |
| `PostHog:PersonalApiKey` | Enables local flag evaluation and private REST API calls |
| `PostHogAdminApi:ProjectId` | Numeric project/environment id for cohorts, actions, dashboards, insights |
| `PostHogAdminApi:AppHostUrl` | App host for private API (`https://us.posthog.com` or `https://eu.posthog.com`) |

**EU cloud:** use `https://eu.i.posthog.com` for ingestion and `https://eu.posthog.com` for the admin API.

Endpoints under `/api/cohorts`, `/api/actions`, `/api/dashboards`, and `/api/insights` return `503` until the personal API key and project id are configured.

## API overview

| Area | Endpoints |
|------|-----------|
| Health | `GET /api/health` |
| Events | `POST /api/events/capture`, `POST /api/events/identify`, `POST /api/events/alias`, `POST /api/events/demo/product-viewed` |
| Feature flags | `POST /api/featureflags/evaluate`, `GET /api/featureflags/check/{flagKey}`, `POST /api/featureflags/experiments/run`, `GET /api/featureflags/gated/new-dashboard` |
| Errors | `POST /api/errors/capture`, `GET /api/errors/demo/trigger` |
| Cohorts | `GET/POST /api/cohorts`, `GET /api/cohorts/{id}` |
| Actions | `GET/POST /api/actions`, `GET /api/actions/{id}` |
| Dashboards | `GET/POST /api/dashboards`, `GET /api/dashboards/{id}`, `POST /api/dashboards/{id}/run-insights` |
| Insights | `GET/POST /api/insights`, `GET /api/insights/{id}` |

Sample HTTP requests are in `src/PostHogSample.Api/PostHogSample.Api.http`.

## Try it locally

```bash
# Capture an event
curl -X POST http://localhost:5032/api/events/capture \
  -H "Content-Type: application/json" \
  -d '{"distinctId":"local-user","eventName":"api tested","properties":{"source":"curl"}}'

# Trigger a demo exception (shows up in PostHog Error Tracking)
curl "http://localhost:5032/api/errors/demo/trigger?distinctId=local-user"

# Check a feature flag (create the flag in PostHog first)
curl "http://localhost:5032/api/featureflags/check/new-dashboard?distinctId=local-user"
```

Pass `X-PostHog-Distinct-Id` on requests to align backend events with a frontend PostHog session.

## Solution structure

```
PostHogSample.sln
src/PostHogSample.Api/
  Controllers/     # REST endpoints per PostHog feature area
  Services/        # PostHog private REST API client
  FeatureManagement/  # .NET Feature Management + PostHog flags
  Models/          # Request DTOs
  Program.cs       # PostHog registration and middleware
```

## Packages

- [PostHog.AspNetCore](https://www.nuget.org/packages/PostHog.AspNetCore) — SDK for events, flags, exceptions
- [Microsoft.FeatureManagement.AspNetCore](https://www.nuget.org/packages/Microsoft.FeatureManagement.AspNetCore) — `[FeatureGate]` integration

## References

- [PostHog .NET docs](https://posthog.com/docs/libraries/dotnet)
- [PostHog API overview](https://posthog.com/docs/api)
