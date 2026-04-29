namespace MOE.Sdk;

/// <summary>
/// Main contract for all MOE plugins.
/// Implement this interface in your plugin's entry class.
/// </summary>
public interface IPlugin : IAsyncDisposable
{
    /// <summary>
    /// Display name of the plugin. Must match the name in service.config.json.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Semantic version of the plugin. Must match the version in service.config.json.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Called once before the first execution.
    /// Use this to read configuration, open connections, warm up caches.
    /// </summary>
    /// <param name="context">Host-provided context with config, logger and cancellation.</param>
    Task InitializeAsync(IPluginContext context);

    /// <summary>
    /// The main execution entry point.
    /// Called by the scheduler according to the plugin's schedule definition.
    /// Must respect the CancellationToken for graceful shutdown and timeout handling.
    /// </summary>
    /// <param name="ct">Linked token: host shutdown + execution timeout + external trigger.</param>
    /// <returns>A PluginResult describing the outcome of this execution.</returns>
    Task<PluginResult> ExecuteAsync(CancellationToken ct);
}

/// <summary>
/// Typed variant of IPlugin for plugins that produce a structured output.
/// The output is included in the PluginResult and persisted in the execution history.
/// </summary>
/// <typeparam name="TOutput">The type of the structured output. Must be JSON-serializable.</typeparam>
public interface IPlugin<TOutput> : IPlugin
{
    /// <summary>
    /// Typed execution entry point.
    /// Replaces ExecuteAsync — the engine calls this and wraps the result automatically.
    /// </summary>
    Task<PluginResult<TOutput>> ExecuteTypedAsync(CancellationToken ct);
}