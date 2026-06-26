using Microsoft.AspNetCore.Mvc;
using PostHogSample.Api.Models;
using PostHogSample.Api.Services;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ActionsController(IPostHogAdminApiClient adminApi) : ControllerBase
{
    /// <summary>
    /// List PostHog actions (grouped event definitions used in insights and funnels).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? limit = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var query = new List<string>();
        if (limit.HasValue)
        {
            query.Add($"limit={limit.Value}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search)}");
        }

        var path = query.Count > 0 ? $"actions/?{string.Join('&', query)}" : "actions/";
        var result = await adminApi.GetProjectAsync(path, cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Get a single action by id.
    /// </summary>
    [HttpGet("{actionId:int}")]
    public async Task<IActionResult> Get(int actionId, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var result = await adminApi.GetProjectAsync($"actions/{actionId}/", cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Create an action that matches events (e.g. product viewed on /pricing).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActionRequest request, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        object[] steps = request.Steps?.Count > 0
            ? request.Steps.Select(step => (object)new { @event = step.Event, url = step.Url }).ToArray()
            : [new { @event = "product viewed" }];

        var body = new
        {
            name = request.Name,
            description = request.Description,
            tags = request.Tags,
            steps,
        };

        var result = await adminApi.PostProjectAsync("actions/", body, cancellationToken);
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
