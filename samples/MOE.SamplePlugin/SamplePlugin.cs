using MOE.Sdk;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace MOE.SamplePlugin;

// -------------------------------------------------------------------------
// Output type — the structured data this plugin produces
// -------------------------------------------------------------------------

/// <summary>
/// The typed output produced by SamplePlugin on each successful run.
/// Any JSON-serializable type works here.
/// </summary>
public record SamplePluginOutput(
    int ItemsProcessed,
    string Summary,
    DateTimeOffset ProcessedAt
);

// -------------------------------------------------------------------------
// Plugin implementation
// -------------------------------------------------------------------------

/// <summary>
/// Reference implementation of a MOE plugin.
/// 
/// This sample demonstrates:
/// - IPlugin&lt;TOutput&gt; with a typed result
/// - Reading configuration from IPluginContext
/// - Proper CancellationToken handling
/// - Correct use of InitializeAsync and DisposeAsync
/// </summary>
public class SamplePlugin : IPlugin<SamplePluginOutput>
{
    // -------------------------------------------------------------------------
    // IPlugin contract
    // -------------------------------------------------------------------------

    public string Name => "Sample Plugin";
    public string Version => "1.0.0";

    // -------------------------------------------------------------------------
    // Private state — initialized in InitializeAsync
    // -------------------------------------------------------------------------

    private ILogger _logger = null!;
    private string _greeting = "Hello";
    private int _itemsToProcess = 10;
    private bool _initialized;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public Task InitializeAsync(IPluginContext context)
    {
        _logger = context.Logger;
        
        // Read plugin-specific config from appsettings.json
        _greeting = context.Configuration["Greeting"] ?? "Hello";
        _itemsToProcess = context.Configuration.GetValue<int>("ItemsToProcess", defaultValue: 10);

        _logger.LogInformation(
            "SamplePlugin initialized. Greeting={Greeting}, ItemsToProcess={Items}",
            _greeting, _itemsToProcess);

        _initialized = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// IPlugin.ExecuteAsync — called by the engine.
    /// Delegates to ExecuteTypedAsync and wraps the result.
    /// </summary>
    public async Task<PluginResult> ExecuteAsync(CancellationToken ct)
    {
        var typed = await ExecuteTypedAsync(ct);
        return typed; // implicit upcast — PluginResult<T> : PluginResult
    }

    /// <summary>
    /// The actual typed execution logic.
    /// Returns a strongly-typed PluginResult&lt;SamplePluginOutput&gt;.
    /// </summary>
    public async Task<PluginResult<SamplePluginOutput>> ExecuteTypedAsync(CancellationToken ct)
    {
        if (!_initialized)
            return PluginResult<SamplePluginOutput>.Fail(
                "Plugin was not initialized. Call InitializeAsync first.", retriable: false);

        _logger.LogInformation("SamplePlugin starting execution. Attempt #{Attempt}",
            // Note: attempt number is available via IPluginContext.ExecutionContext
            1);

        try
        {
            var processedCount = 0;

            for (var i = 0; i < _itemsToProcess; i++)
            {
                // Always check for cancellation inside loops
                ct.ThrowIfCancellationRequested();

                // Simulate work
                await Task.Delay(100, ct);

                processedCount++;
                _logger.LogDebug("{Greeting} from item {Index}", _greeting, i + 1);
            }

            var output = new SamplePluginOutput(
                ItemsProcessed: processedCount,
                Summary: $"{_greeting} — processed {processedCount} items successfully.",
                ProcessedAt: DateTimeOffset.UtcNow
            );

            _logger.LogInformation("SamplePlugin completed. {ItemsProcessed} items processed.",
                processedCount);

            return PluginResult<SamplePluginOutput>.Ok(output);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SamplePlugin execution was cancelled.");
            return PluginResult<SamplePluginOutput>.Fail(
                "Execution was cancelled by the host.", retriable: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SamplePlugin execution failed with an unexpected error.");
            return PluginResult<SamplePluginOutput>.Fail(ex, retriable: true);
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    public ValueTask DisposeAsync()
    {
        _logger?.LogInformation("SamplePlugin disposed.");
        // Release any resources opened in InitializeAsync here
        // e.g.: await _dbConnection.DisposeAsync();
        return ValueTask.CompletedTask;
    }
}