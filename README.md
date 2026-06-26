# PostHog Sample .NET Web API

A minimal ASP.NET Core Web API that demonstrates [PostHog](https://posthog.com) integration for:

- **Events & properties** — capture, identify, alias
- **Actions & cohorts** — read/create via PostHog private REST API
- **Dashboards & insights** — list, create, and run dashboard insights
- **Feature flags & experiments** — evaluate flags and A/B variants
- **Error tracking** — manual `$exception` capture

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (or .NET 8+ with a matching target framework)
- A PostHog Cloud or self-hosted project

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
