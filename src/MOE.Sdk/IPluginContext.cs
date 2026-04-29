using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MOE.Sdk;

/// <summary>
/// Context provided by MOE Host to the plugin during initialization and execution.
/// This is the only surface through which a plugin interacts with the host.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Unique identifier of this plugin instance, as defined in service.config.json.
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// Semantic version of the running plugin.
    /// </summary>
    string PluginVersion { get; }

    /// <summary>
    /// Configuration loaded from the plugin's appsettings.json (if present).
    /// Falls back to an empty configuration if the file does not exist.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Structured logger scoped to this plugin.
    /// All log entries are persisted to LiteDB with the PluginId as the correlation key.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Fired when the MOE Host is shutting down gracefully.
    /// Plugins should monitor this token alongside their execution CancellationToken.
    /// </summary>
    CancellationToken HostShutdownToken { get; }

    /// <summary>
    /// Metadata about the current execution (attempt number, trigger type, scheduled time).
    /// </summary>
    ExecutionContext ExecutionContext { get; }
}

/// <summary>
/// Metadata about the current plugin execution provided by the scheduler.
/// </summary>
public record ExecutionContext
{
    /// <summary>Unique ID of this execution run.</summary>
    public required Guid ExecutionId { get; init; }

    /// <summary>How this execution was triggered.</summary>
    public required TriggerType TriggerType { get; init; }

    /// <summary>The time at which this execution was originally scheduled.</summary>
    public required DateTimeOffset ScheduledAt { get; init; }

    /// <summary>Current attempt number. Starts at 1, increments on retry.</summary>
    public required int AttemptNumber { get; init; }

    /// <summary>True if this is a retry of a previous failed execution.</summary>
    public bool IsRetry => AttemptNumber > 1;
}

/// <summary>
/// Describes how a plugin execution was initiated.
/// </summary>
public enum TriggerType
{
    /// <summary>Scheduled automatically by the cron expression or interval.</summary>
    Scheduled,

    /// <summary>Triggered via the REST API trigger endpoint.</summary>
    ApiTrigger,

    /// <summary>Executed immediately via the manual run endpoint.</summary>
    Manual,

    /// <summary>Retried automatically after a previous failure.</summary>
    Retry
}