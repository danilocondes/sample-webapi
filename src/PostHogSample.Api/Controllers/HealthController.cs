using Microsoft.AspNetCore.Mvc;
using PostHogSample.Api.Services;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController(IPostHogAdminApiClient adminApi, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new
        {
            service = "PostHog Sample API",
            status = "healthy",
            postHog = new
            {
                projectTokenConfigured = !string.IsNullOrWhiteSpace(
                    configuration["PostHog:ProjectToken"]
                    ?? configuration["PostHog:ProjectApiKey"]),
                hostUrl = configuration["PostHog:HostUrl"] ?? configuration["PostHog:Host"] ?? "https://us.i.posthog.com",
                adminApiConfigured = adminApi.IsConfigured,
            },
            endpoints = new
            {
                events = "/api/events",
                featureFlags = "/api/featureflags",
                errors = "/api/errors",
                cohorts = "/api/cohorts",
                actions = "/api/actions",
                dashboards = "/api/dashboards",
                insights = "/api/insights",
            },
        });
}
