namespace Shared.Api;

/// <summary>
/// Standard API error envelope returned by all endpoints on failure.
/// Provides a consistent error shape for clients to parse.
/// </summary>
/// <param name="Code">Machine-readable error code (e.g. "NOT_FOUND").</param>
/// <param name="Message">Human-readable description of the error.</param>
public record ApiError(string Code, string Message)
{
    /// <summary>Optional list of field-level validation messages.</summary>
    public IReadOnlyList<string>? Details { get; init; }

    // ── Factory helpers ───────────────────────────────────────────────────────

    public static ApiError NotFound(string resource, object id)
        => new("NOT_FOUND", $"{resource} '{id}' not found.");

    public static ApiError BadRequest(string message)
        => new("BAD_REQUEST", message);

    public static ApiError Conflict(string message)
        => new("CONFLICT", message);

    public static ApiError UnprocessableEntity(string message)
        => new("UNPROCESSABLE_ENTITY", message);

    /// <summary>For 422 responses when multiple fields fail validation.</summary>
    public static ApiError Validation(IReadOnlyList<string> details)
        => new("VALIDATION_ERROR", "One or more validation errors occurred.") { Details = details };
}
