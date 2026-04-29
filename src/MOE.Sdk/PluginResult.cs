namespace MOE.Sdk;

/// <summary>
/// Describes the outcome of a plugin execution.
/// Use the static factory methods to create instances.
/// </summary>
public record PluginResult
{
    /// <summary>Whether the execution completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable description of the outcome.</summary>
    public string? Message { get; init; }

    /// <summary>
    /// If the execution failed, indicates whether the engine should schedule a retry.
    /// Ignored when Success is true.
    /// </summary>
    public bool IsRetriable { get; init; }

    /// <summary>
    /// If the execution failed, contains the exception that caused the failure.
    /// Not serialized to the database — use Message for persistent error description.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Duration of the execution, set by the runner after ExecuteAsync completes.
    /// Plugins do not set this directly.
    /// </summary>
    public TimeSpan? Duration { get; internal set; }

    // -------------------------------------------------------------------------
    // Factory methods
    // -------------------------------------------------------------------------

    /// <summary>Creates a successful result with an optional message.</summary>
    public static PluginResult Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>Creates a failed result. Retriable by default.</summary>
    public static PluginResult Fail(string message, bool retriable = true) => new()
    {
        Success = false,
        Message = message,
        IsRetriable = retriable
    };

    /// <summary>Creates a failed result from an exception. Retriable by default.</summary>
    public static PluginResult Fail(Exception ex, bool retriable = true) => new()
    {
        Success = false,
        Message = ex.Message,
        Exception = ex,
        IsRetriable = retriable
    };

    /// <summary>
    /// Creates a permanently failed result that will never be retried.
    /// Use for business logic failures where retrying would not help.
    /// </summary>
    public static PluginResult Fatal(string message) => new()
    {
        Success = false,
        Message = message,
        IsRetriable = false
    };

    /// <summary>
    /// Creates a result indicating the execution was cancelled (host shutdown or timeout).
    /// Always treated as retriable.
    /// </summary>
    public static PluginResult Cancelled(string? reason = null) => new()
    {
        Success = false,
        Message = reason ?? "Execution was cancelled.",
        IsRetriable = true
    };
}

/// <summary>
/// Typed variant of PluginResult that carries a structured output payload.
/// Use this when the plugin produces data that should be persisted or forwarded.
/// </summary>
/// <typeparam name="TOutput">JSON-serializable output type.</typeparam>
public record PluginResult<TOutput> : PluginResult
{
    /// <summary>
    /// The structured output produced by the plugin.
    /// Null when the execution failed.
    /// </summary>
    public TOutput? Output { get; init; }

    // -------------------------------------------------------------------------
    // Factory methods
    // -------------------------------------------------------------------------

    /// <summary>Creates a successful result with a typed output payload.</summary>
    public static PluginResult<TOutput> Ok(TOutput output, string? message = null) => new()
    {
        Success = true,
        Output = output,
        Message = message
    };

    /// <summary>Creates a failed typed result with no output.</summary>
    public new static PluginResult<TOutput> Fail(string message, bool retriable = true) => new()
    {
        Success = false,
        Message = message,
        IsRetriable = retriable,
        Output = default
    };

    /// <summary>Creates a failed typed result from an exception.</summary>
    public new static PluginResult<TOutput> Fail(Exception ex, bool retriable = true) => new()
    {
        Success = false,
        Message = ex.Message,
        Exception = ex,
        IsRetriable = retriable,
        Output = default
    };

    /// <summary>Converts a typed result to its untyped base form.</summary>
    public PluginResult ToBase() => this with { Output = default };
}