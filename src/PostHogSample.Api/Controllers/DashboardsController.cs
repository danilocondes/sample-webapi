using Microsoft.AspNetCore.Mvc;
using PostHogSample.Api.Models;
using PostHogSample.Api.Services;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardsController(IPostHogAdminApiClient adminApi) : ControllerBase
{
    /// <summary>
    /// List dashboards in your PostHog environment.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? limit = 20, CancellationToken cancellationToken = default)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var path = limit.HasValue ? $"dashboards/?limit={limit.Value}" : "dashboards/";
        var result = await adminApi.GetEnvironmentAsync(path, cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Get a dashboard by id.
    /// </summary>
    [HttpGet("{dashboardId:int}")]
    public async Task<IActionResult> Get(int dashboardId, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var result = await adminApi.GetEnvironmentAsync($"dashboards/{dashboardId}/", cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Create a dashboard shell you can populate in the PostHog UI.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDashboardRequest request, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var body = new
        {
            name = request.Name,
            description = request.Description,
        };

        var result = await adminApi.PostEnvironmentAsync("dashboards/", body, cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Run all insights on a dashboard and return computed results.
    /// </summary>
    [HttpPost("{dashboardId:int}/run-insights")]
    public async Task<IActionResult> RunInsights(int dashboardId, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var result = await adminApi.PostEnvironmentAsync(
            $"dashboards/{dashboardId}/run_insights/",
            new { },
            cancellationToken);

        return Content(result.RootElement.GetRawText(), "application/json");
    }

    private ObjectResult ServiceUnavailable() =>
        StatusCode(503, new
        {
            message = "PostHog admin API is not configured.",
            required = new[]
            {
                "PostHogAdminApi:ProjectId",
                "PostHogAdminApi:PersonalApiKey (or PostHog:PersonalApiKey)",
            },
        });
}
