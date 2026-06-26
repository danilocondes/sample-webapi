using PostHog;
using PostHog.FeatureManagement;

namespace PostHogSample.Api.FeatureManagement;

/// <summary>
/// Supplies distinct_id and person properties for .NET Feature Management + PostHog flags.
/// </summary>
public sealed class ApiFeatureFlagContextProvider(IHttpContextAccessor httpContextAccessor)
    : PostHogFeatureFlagContextProvider
{
    protected override string? GetDistinctId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        if (context.Request.Headers.TryGetValue("X-PostHog-Distinct-Id", out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        if (context.Request.Query.TryGetValue("distinctId", out var queryValue)
            && !string.IsNullOrWhiteSpace(queryValue))
        {
            return queryValue.ToString();
        }

        return context.Request.Headers.TryGetValue("X-Distinct-Id", out var fallback)
            ? fallback.ToString()
            : "anonymous-api-user";
    }

    protected override ValueTask<FeatureFlagOptions> GetFeatureFlagOptionsAsync()
    {
        var context = httpContextAccessor.HttpContext;
        var personProperties = new Dictionary<string, object?>();

        if (context?.Request.Query.TryGetValue("plan", out var plan) == true
            && !string.IsNullOrWhiteSpace(plan))
        {
            personProperties["plan"] = plan.ToString();
        }

        if (context?.Request.Query.TryGetValue("email", out var email) == true
            && !string.IsNullOrWhiteSpace(email))
        {
            personProperties["email"] = email.ToString();
        }

        return ValueTask.FromResult(new FeatureFlagOptions
        {
            PersonProperties = personProperties,
            OnlyEvaluateLocally = false,
        });
    }
}
