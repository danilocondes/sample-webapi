using Microsoft.AspNetCore.Mvc;
using PostHogSample.Api.Models;
using PostHogSample.Api.Services;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InsightsController(IPostHogAdminApiClient adminApi) : ControllerBase
{
    /// <summary>
    /// List saved insights in your PostHog environment.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? limit = 20, CancellationToken cancellationToken = default)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var path = limit.HasValue ? $"insights/?limit={limit.Value}" : "insights/";
        var result = await adminApi.GetEnvironmentAsync(path, cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Get a saved insight by id.
    /// </summary>
    [HttpGet("{insightId:int}")]
    public async Task<IActionResult> Get(int insightId, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var result = await adminApi.GetEnvironmentAsync($"insights/{insightId}/", cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Create a simple trends insight for an event.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInsightRequest request, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var body = new
        {
            name = request.Name,
            description = request.Description,
            filters = new
            {
                insight = "TRENDS",
                events = new[]
                {
                    new { id = request.EventName, type = "events", order = 0 },
                },
                date_from = "-7d",
            },
        };

        var result = await adminApi.PostEnvironmentAsync("insights/", body, cancellationToken);
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
