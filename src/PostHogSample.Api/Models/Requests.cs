namespace PostHogSample.Api.Models;

public sealed record CaptureEventRequest(
    string DistinctId,
    string EventName,
    Dictionary<string, object?>? Properties = null,
    Dictionary<string, object?>? PersonPropertiesToSet = null,
    Dictionary<string, object?>? PersonPropertiesToSetOnce = null);

public sealed record IdentifyUserRequest(
    string DistinctId,
    string? Email = null,
    string? Name = null,
    Dictionary<string, object?>? PersonPropertiesToSet = null,
    Dictionary<string, object?>? PersonPropertiesToSetOnce = null);

public sealed record AliasUserRequest(string DistinctId, string Alias);

public sealed record EvaluateFlagsRequest(
    string DistinctId,
    string[]? FlagKeysToEvaluate = null,
    Dictionary<string, object?>? PersonProperties = null);

public sealed record ExperimentRequest(
    string DistinctId,
    string ExperimentFlagKey,
    Dictionary<string, object?>? PersonProperties = null);

public sealed record CaptureExceptionRequest(
    string DistinctId,
    string Message,
    string? ExceptionType = null,
    Dictionary<string, object?>? Properties = null);

public sealed record CreateActionRequest(
    string Name,
    string? Description = null,
    IReadOnlyList<ActionStepRequest>? Steps = null,
    IReadOnlyList<string>? Tags = null);

public sealed record ActionStepRequest(
    string Event,
    string? Url = null);

public sealed record CreateCohortRequest(
    string Name,
    string? Description = null,
    bool IsStatic = false);

public sealed record CreateDashboardRequest(
    string Name,
    string? Description = null);

public sealed record CreateInsightRequest(
    string Name,
    string EventName,
    string? Description = null);
