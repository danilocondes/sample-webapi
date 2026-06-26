using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using PostHog;
using PostHogSample.Api.Models;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeatureFlagsController(IPostHogClient postHog, IFeatureManager featureManager) : ControllerBase
{
    /// <summary>
    /// Evaluate feature flags for a user. Returns all evaluated flag values.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] EvaluateFlagsRequest request, CancellationToken cancellationToken)
    {
        var options = request.FlagKeysToEvaluate is { Length: > 0 }
            ? new AllFeatureFlagsOptions
            {
                FlagKeysToEvaluate = request.FlagKeysToEvaluate,
                PersonProperties = request.PersonProperties,
            }
            : new AllFeatureFlagsOptions
            {
                PersonProperties = request.PersonProperties,
            };

        var flags = await postHog.EvaluateFlagsAsync(
            request.DistinctId,
            options: options,
            cancellationToken: cancellationToken);

        var result = new Dictionary<string, object?>();
        foreach (var key in request.FlagKeysToEvaluate ?? [])
        {
            var flag = flags.GetFlag(key);
            result[key] = flag?.VariantKey ?? (flag?.IsEnabled == true ? "true" : "false");
        }

        if (request.FlagKeysToEvaluate is null or { Length: 0 })
        {
            return Ok(new
            {
                distinctId = request.DistinctId,
                message = "Pass FlagKeysToEvaluate to return specific flags, or use /api/featureflags/check/{flagKey}.",
            });
        }

        return Ok(new { distinctId = request.DistinctId, flags = result });
    }

    /// <summary>
    /// Check a single boolean or multivariate feature flag.
    /// </summary>
    [HttpGet("check/{flagKey}")]
    public async Task<IActionResult> CheckFlag(
        string flagKey,
        [FromQuery] string distinctId,
        CancellationToken cancellationToken)
    {
        var flags = await postHog.EvaluateFlagsAsync(
            distinctId,
            options: new AllFeatureFlagsOptions { FlagKeysToEvaluate = [flagKey] },
            cancellationToken: cancellationToken);

        var flag = flags.GetFlag(flagKey);
        return Ok(new
        {
            distinctId,
            flagKey,
            enabled = flags.IsEnabled(flagKey),
            variant = flag?.VariantKey,
            payload = flags.GetFlagPayload(flagKey),
        });
    }

    /// <summary>
    /// Run an A/B experiment by reading a multivariate feature flag variant.
    /// Create the experiment in PostHog UI and use its feature flag key here.
    /// </summary>
    [HttpPost("experiments/run")]
    public async Task<IActionResult> RunExperiment([FromBody] ExperimentRequest request, CancellationToken cancellationToken)
    {
        var options = new AllFeatureFlagsOptions
        {
            FlagKeysToEvaluate = [request.ExperimentFlagKey],
            PersonProperties = request.PersonProperties,
        };

        var flags = await postHog.EvaluateFlagsAsync(
            request.DistinctId,
            options: options,
            cancellationToken: cancellationToken);

        var variant = flags.GetFlag(request.ExperimentFlagKey)?.VariantKey ?? "control";
        var experience = variant switch
        {
            "variant-b" => "Experience B: bold checkout CTA",
            "variant-a" => "Experience A: streamlined checkout",
            _ => "Control: default checkout",
        };

        postHog.Capture(
            request.DistinctId,
            "experiment exposure",
            properties: new Dictionary<string, object>
            {
                ["experiment_flag"] = request.ExperimentFlagKey,
                ["variant"] = variant,
                ["$feature/" + request.ExperimentFlagKey] = variant,
            },
            groups: null,
            flags: flags.Only(request.ExperimentFlagKey));

        return Ok(new
        {
            request.DistinctId,
            request.ExperimentFlagKey,
            variant,
            experience,
        });
    }

    /// <summary>
    /// Endpoint gated by .NET Feature Management + PostHog (flag: new-dashboard).
    /// Enable the flag in PostHog to access this route.
    /// </summary>
    [HttpGet("gated/new-dashboard")]
    [FeatureGate("new-dashboard")]
    public IActionResult GatedNewDashboard() =>
        Ok(new { message = "You have access to the new dashboard feature flag." });

    /// <summary>
    /// Same gating via IFeatureManager for programmatic checks.
    /// </summary>
    [HttpGet("gated/pricing-page")]
    public async Task<IActionResult> GatedPricingPage([FromQuery] string? distinctId = null)
    {
        var enabled = await featureManager.IsEnabledAsync("pricing-page-redesign");
        if (!enabled)
        {
            return NotFound(new { message = "pricing-page-redesign flag is disabled for this user." });
        }

        return Ok(new { message = "Pricing page redesign is enabled.", distinctId });
    }
}
