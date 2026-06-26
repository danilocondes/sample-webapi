using Microsoft.AspNetCore.Mvc;
using PostHog;
using PostHogSample.Api.Models;

namespace PostHogSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ErrorsController(IPostHogClient postHog, ILogger<ErrorsController> logger) : ControllerBase
{
    /// <summary>
    /// Manually capture an exception for PostHog error tracking ($exception event).
    /// </summary>
    [HttpPost("capture")]
    public IActionResult CaptureException([FromBody] CaptureExceptionRequest request)
    {
        var exception = string.IsNullOrWhiteSpace(request.ExceptionType)
            ? new InvalidOperationException(request.Message)
            : (Exception)Activator.CreateInstance(Type.GetType(request.ExceptionType) ?? typeof(Exception), request.Message)!;

        postHog.CaptureException(exception, request.DistinctId, ToObjectDictionary(request.Properties));

        return Accepted(new
        {
            message = "Exception captured for PostHog error tracking",
            request.DistinctId,
            exceptionType = exception.GetType().Name,
        });
    }

    /// <summary>
    /// Demo endpoint that throws and captures a real exception with request context.
    /// </summary>
    [HttpGet("demo/trigger")]
    public IActionResult TriggerDemoException([FromQuery] string distinctId = "demo-user")
    {
        try
        {
            throw new ApplicationException("Demo failure from PostHog sample API");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Demo exception triggered for distinctId {DistinctId}", distinctId);

            postHog.CaptureException(
                ex,
                distinctId,
                new Dictionary<string, object>
                {
                    ["demo"] = true,
                    ["endpoint"] = "/api/errors/demo/trigger",
                });

            return StatusCode(500, new
            {
                message = "A demo exception was thrown and sent to PostHog.",
                distinctId,
                hint = "Check Error Tracking in your PostHog project.",
            });
        }
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
