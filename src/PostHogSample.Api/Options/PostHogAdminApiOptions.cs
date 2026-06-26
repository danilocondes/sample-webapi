namespace PostHogSample.Api.Options;

/// <summary>
/// Settings for PostHog private REST API calls (cohorts, actions, dashboards, insights).
/// Requires a personal API key with the appropriate scopes.
/// </summary>
public sealed class PostHogAdminApiOptions
{
    public const string SectionName = "PostHogAdminApi";

    /// <summary>
    /// PostHog app host used for private API routes, e.g. https://us.posthog.com.
    /// </summary>
    public string AppHostUrl { get; set; } = "https://us.posthog.com";

    /// <summary>
    /// Numeric project/environment id from PostHog project settings.
    /// </summary>
    public string ProjectId { get; set; } = "";

    /// <summary>
    /// Personal API key (phx_...) with cohort, action, insight, and dashboard scopes.
    /// Prefer user-secrets or environment variables in production.
    /// </summary>
    public string? PersonalApiKey { get; set; }
}
