using Microsoft.AspNetCore.Mvc;
using PostHog;
using PostHogSample.Api.Models;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventsController(IPostHogClient postHog) : ControllerBase
{
    /// <summary>
    /// Capture a custom event with optional event and person properties.
    /// </summary>
    [HttpPost("capture")]
    public async Task<IActionResult> Capture([FromBody] CaptureEventRequest request, CancellationToken cancellationToken)
    {
        if (request.PersonPropertiesToSet is not null || request.PersonPropertiesToSetOnce is not null)
        {
            await postHog.IdentifyAsync(
                request.DistinctId,
                ToObjectDictionary(request.PersonPropertiesToSet),
                ToObjectDictionary(request.PersonPropertiesToSetOnce),
                cancellationToken);
        }

        var flags = await postHog.EvaluateFlagsAsync(
            request.DistinctId,
            cancellationToken: cancellationToken);

        postHog.Capture(
            request.DistinctId,
            request.EventName,
            properties: ToObjectDictionary(request.Properties),
            groups: null,
            flags: flags.OnlyAccessed());

        return Accepted(new
        {
            message = "Event queued for PostHog",
            request.EventName,
            request.DistinctId,
        });
    }

    /// <summary>
    /// Identify a user and set person properties ($set / $set_once).
    /// </summary>
    [HttpPost("identify")]
    public async Task<IActionResult> Identify([FromBody] IdentifyUserRequest request, CancellationToken cancellationToken)
    {
        var propertiesToSet = ToObjectDictionary(request.PersonPropertiesToSet) ?? new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            propertiesToSet["email"] = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            propertiesToSet["name"] = request.Name;
        }

        await postHog.IdentifyAsync(
            request.DistinctId,
            propertiesToSet,
            ToObjectDictionary(request.PersonPropertiesToSetOnce),
            cancellationToken);

        return Accepted(new { message = "User identified", request.DistinctId });
    }

    /// <summary>
    /// Link two distinct IDs for the same person.
    /// </summary>
    [HttpPost("alias")]
    public async Task<IActionResult> Alias([FromBody] AliasUserRequest request, CancellationToken cancellationToken)
    {
        await postHog.AliasAsync(request.DistinctId, request.Alias, cancellationToken);
        return Accepted(new { message = "Alias created", request.DistinctId, request.Alias });
    }

    /// <summary>
    /// Demo endpoint that captures a product action with rich properties.
    /// </summary>
    [HttpPost("demo/product-viewed")]
    public async Task<IActionResult> DemoProductViewed(
        [FromQuery] string distinctId,
        [FromQuery] string productId,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var properties = new Dictionary<string, object>
        {
            ["product_id"] = productId,
            ["category"] = category ?? "general",
            ["source"] = "dotnet-sample-api",
        };

        await postHog.IdentifyAsync(
            distinctId,
            new Dictionary<string, object> { ["last_product_viewed"] = productId },
            personPropertiesToSetOnce: null,
            cancellationToken);

        var flags = await postHog.EvaluateFlagsAsync(distinctId, cancellationToken: cancellationToken);

        postHog.Capture(
            distinctId,
            "product viewed",
            properties: properties,
            groups: null,
            flags: flags.OnlyAccessed());

        return Ok(new { captured = "product viewed", distinctId, properties });
    }

    private static Dictionary<string, object>? ToObjectDictionary(Dictionary<string, object?>? source)
    {
        if (source is null)
        {
            return null;
        }

        return source.ToDictionary(static pair => pair.Key, static pair => pair.Value ?? string.Empty);
    }
}
