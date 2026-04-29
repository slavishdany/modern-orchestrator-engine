using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MOE.Sdk;

/// <summary>
/// Represents the content of a plugin's service.config.json file.
/// This is the "passport" of a plugin — read by the host before loading any DLL.
/// </summary>
public class PluginManifest
{
    /// <summary>
    /// Unique identifier for this plugin. Used as the primary key in the database.
    /// Must be lowercase, hyphen-separated. Example: "invoice-exporter"
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Id must be lowercase alphanumeric with hyphens (e.g. 'my-plugin').")]
    public required string Id { get; init; }

    /// <summary>Display name shown in the UI and logs.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Semantic version. Must match IPlugin.Version at runtime.</summary>
    [Required]
    public required string Version { get; init; }

    /// <summary>
    /// Fully qualified type name of the class implementing IPlugin.
    /// Example: "Acme.Plugins.InvoiceExporter"
    /// </summary>
    [Required]
    public required string EntryType { get; init; }

    /// <summary>Scheduling configuration for this plugin.</summary>
    [Required]
    public required ScheduleConfig Schedule { get; init; }

    /// <summary>Retry policy applied on failure.</summary>
    public RetryConfig Retry { get; init; } = new();

    /// <summary>Execution timeout configuration.</summary>
    public TimeoutConfig Timeout { get; init; } = new();

    /// <summary>Security capabilities declared by the plugin.</summary>
    public CapabilitiesConfig Capabilities { get; init; } = new();

    /// <summary>Optional tags for grouping and filtering in the UI.</summary>
    public string[] Tags { get; init; } = [];

    /// <summary>Optional human-readable description of what this plugin does.</summary>
    public string? Description { get; init; }
}

// -------------------------------------------------------------------------
// Schedule
// -------------------------------------------------------------------------

/// <summary>
/// Defines when and how often the plugin should be executed.
/// Either Cron or IntervalSeconds must be set — not both.
/// </summary>
public class ScheduleConfig
{
    /// <summary>
    /// Cron expression (6-part with seconds, or standard 5-part).
    /// Example: "0 2 * * *" (every day at 02:00)
    /// </summary>
    public string? Cron { get; init; }

    /// <summary>
    /// Simple interval in seconds. Mutually exclusive with Cron.
    /// Example: 300 (every 5 minutes)
    /// </summary>
    public int? IntervalSeconds { get; init; }

    /// <summary>
    /// If true, the plugin will be executed once at startup before the schedule kicks in.
    /// Defaults to false.
    /// </summary>
    public bool RunOnStartup { get; init; } = false;

    /// <summary>Returns true if the schedule configuration is valid.</summary>
    [JsonIgnore]
    public bool IsValid =>
        (Cron is not null) ^ (IntervalSeconds is not null);

    /// <summary>Returns the schedule type for display purposes.</summary>
    [JsonIgnore]
    public ScheduleType Type => Cron is not null ? ScheduleType.Cron : ScheduleType.Interval;
}

public enum ScheduleType { Cron, Interval }

// -------------------------------------------------------------------------
// Retry
// -------------------------------------------------------------------------

/// <summary>Defines how the engine retries a failed execution.</summary>
public class RetryConfig
{
    /// <summary>Maximum number of attempts including the first one. Default: 3.</summary>
    public int MaxAttempts { get; init; } = 3;

    /// <summary>Delay in seconds before the first retry. Default: 30.</summary>
    public int DelaySeconds { get; init; } = 30;

    /// <summary>Backoff strategy applied to DelaySeconds on each subsequent retry.</summary>
    public BackoffStrategy Backoff { get; init; } = BackoffStrategy.Exponential;
}

public enum BackoffStrategy
{
    /// <summary>Same delay on every retry.</summary>
    Fixed,

    /// <summary>Delay doubles on each retry: 30s → 60s → 120s.</summary>
    Exponential,

    /// <summary>Delay increases linearly: 30s → 60s → 90s.</summary>
    Linear
}

// -------------------------------------------------------------------------
// Timeout
// -------------------------------------------------------------------------

/// <summary>Defines the maximum allowed execution time.</summary>
public class TimeoutConfig
{
    /// <summary>
    /// Maximum execution time in seconds before the engine cancels the plugin.
    /// Default: 300 (5 minutes). Set to 0 to disable timeout (not recommended).
    /// </summary>
    public int ExecutionSeconds { get; init; } = 300;
}

// -------------------------------------------------------------------------
// Capabilities (Security Declaration)
// -------------------------------------------------------------------------

/// <summary>
/// Explicit declaration of the capabilities the plugin requires.
/// The static analyzer verifies that the DLL matches these declarations.
/// Undeclared capabilities trigger an automatic security rejection.
/// </summary>
public class CapabilitiesConfig
{
    /// <summary>
    /// Reflection usage level.
    /// The static analyzer enforces that the actual usage matches this declaration.
    /// </summary>
    public ReflectionUsage Reflection { get; init; } = ReflectionUsage.None;

    /// <summary>
    /// Filesystem paths the plugin is allowed to read from.
    /// Empty means no filesystem read access.
    /// </summary>
    public string[] FilesystemRead { get; init; } = [];

    /// <summary>
    /// Filesystem paths the plugin is allowed to write to.
    /// Empty means no filesystem write access.
    /// </summary>
    public string[] FilesystemWrite { get; init; } = [];

    /// <summary>
    /// Hostnames the plugin is allowed to connect to.
    /// Empty means no outbound network access.
    /// </summary>
    public string[] AllowedHosts { get; init; } = [];

    /// <summary>
    /// NuGet packages used by this plugin beyond the MOE.Sdk.
    /// Each entry is verified against the actual DLL references by the static analyzer.
    /// </summary>
    public DependencyDeclaration[] ExternalDependencies { get; init; } = [];
}

public enum ReflectionUsage
{
    /// <summary>No reflection used.</summary>
    None,

    /// <summary>Reflection used only on types defined within the plugin itself.</summary>
    Internal,

    /// <summary>Uses Activator.CreateInstance or similar factory patterns.</summary>
    Activator,

    /// <summary>Advanced reflection usage — requires explicit admin approval.</summary>
    Advanced
}

/// <summary>A declared NuGet dependency with its intended purpose.</summary>
public record DependencyDeclaration(
    string Name,
    string Version,
    string Purpose
);