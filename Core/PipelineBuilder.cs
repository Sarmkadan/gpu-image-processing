#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Core
{
    /// <summary>
    /// Fluent builder for composing GPU image-processing pipelines.
    /// Each stage is executed sequentially, passing its output buffer to the next.
    /// </summary>
    public class PipelineBuilder
    {
        private readonly ILogger<PipelineBuilder> _logger;
        private readonly List<PipelineStage> _stages = new();
        private bool _isBuilt;

        /// <summary>
        /// Initializes a new pipeline builder.
        /// </summary>
        /// <param name="logger">Logger for pipeline diagnostics.</param>
        public PipelineBuilder(ILogger<PipelineBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Appends a named processing stage with the given execution delegate.
        /// </summary>
        /// <param name="name">Human-readable stage name for logging.</param>
        /// <param name="execute">Async delegate that processes a byte buffer and returns the result.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder AddStage(string name, Func<byte[], CancellationToken, Task<byte[]>> execute)
        {
            if (_isBuilt)
                throw new InvalidOperationException("Cannot modify a pipeline after it has been built.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Stage name is required.", nameof(name));

            _stages.Add(new PipelineStage(name, execute ?? throw new ArgumentNullException(nameof(execute))));
            _logger.LogDebug("Added pipeline stage: {Stage} (position {Pos})", name, _stages.Count);
            return this;
        }

        /// <summary>
        /// Conditionally appends a stage only when the predicate is true.
        /// </summary>
        public PipelineBuilder AddStageIf(bool condition, string name, Func<byte[], CancellationToken, Task<byte[]>> execute)
        {
            return condition ? AddStage(name, execute) : this;
        }

        /// <summary>
        /// Finalizes the pipeline and returns the ordered list of stages.
        /// After calling Build, no more stages can be added.
        /// </summary>
        public IReadOnlyList<PipelineStage> Build()
        {
            if (_stages.Count == 0)
                throw new InvalidOperationException("Pipeline must contain at least one stage.");

            _isBuilt = true;
            _logger.LogInformation("Pipeline built with {Count} stage(s)", _stages.Count);
            return _stages.AsReadOnly();
        }

        /// <summary>
        /// Executes all stages sequentially, feeding each output into the next.
        /// </summary>
        /// <param name="input">Raw input image data.</param>
        /// <param name="cancellationToken">Token to cancel long-running pipelines.</param>
        /// <returns>Final processed image data.</returns>
        public async Task<byte[]> ExecuteAsync(byte[] input, CancellationToken cancellationToken = default)
        {
            if (!_isBuilt)
                Build();

            var buffer = input ?? throw new ArgumentNullException(nameof(input));

            for (int i = 0; i < _stages.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var stage = _stages[i];

                _logger.LogDebug("Executing stage {Index}/{Total}: {Name}", i + 1, _stages.Count, stage.Name);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                buffer = await stage.Execute(buffer, cancellationToken).ConfigureAwait(false);

                sw.Stop();
                _logger.LogDebug("Stage {Name} completed in {Ms}ms, output {Len} bytes",
                    stage.Name, sw.ElapsedMilliseconds, buffer.Length);
            }

            _logger.LogInformation("Pipeline execution completed, final output {Len} bytes", buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Represents a single named stage in the processing pipeline.
        /// </summary>
        public sealed record PipelineStage(string Name, Func<byte[], CancellationToken, Task<byte[]>> Execute);
    }
}
