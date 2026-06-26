using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PostHogSample.Api.Models;
using PostHogSample.Api.Services;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CohortsController(IPostHogAdminApiClient adminApi) : ControllerBase
{
    /// <summary>
    /// List cohorts in your PostHog project (requires personal API key with cohort:read).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? limit = 20, CancellationToken cancellationToken = default)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var path = limit.HasValue ? $"cohorts/?limit={limit.Value}" : "cohorts/";
        var result = await adminApi.GetProjectAsync(path, cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Get a single cohort by id.
    /// </summary>
    [HttpGet("{cohortId:int}")]
    public async Task<IActionResult> Get(int cohortId, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var result = await adminApi.GetProjectAsync($"cohorts/{cohortId}/", cancellationToken);
        return Content(result.RootElement.GetRawText(), "application/json");
    }

    /// <summary>
    /// Create a static cohort shell (add members via PostHog UI or additional API calls).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCohortRequest request, CancellationToken cancellationToken)
    {
        if (!adminApi.IsConfigured)
        {
            return ServiceUnavailable();
        }

        var body = new
        {
            name = request.Name,
            description = request.Description,
            is_static = request.IsStatic,
        };

        var result = await adminApi.PostProjectAsync("cohorts/", body, cancellationToken);
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
