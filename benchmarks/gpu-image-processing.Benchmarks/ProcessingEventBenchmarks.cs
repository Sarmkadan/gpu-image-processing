using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Events;
using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Benchmarks
{
    [MemoryDiagnoser]
    public class ProcessingEventBenchmarks
    {
        [Params(10, 100, 1000)]
        public int N { get; set; }

        private List<string> _filterIds;
        private List<string> _transformIds;
        private Dictionary<string, object> _metrics;

        [GlobalSetup]
        public void Setup()
        {
            _filterIds = new List<string> { "filter_grayscale", "filter_blur" };
            _transformIds = new List<string> { "transform_resize" };
            _metrics = new Dictionary<string, object>
            {
                { "quality", 95 },
                { "format", "png" }
            };
        }

        [Benchmark]
        public void Instantiate_JobStartedEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = new JobStartedEvent
                {
                    JobId = Guid.NewGuid().ToString(),
                    JobName = "Batch Processing Job",
                    TotalImages = 500,
                    FilterIds = _filterIds,
                    TransformIds = _transformIds
                };
                // Consume to prevent dead code elimination
                _ = ev.EventId;
            }
        }

        [Benchmark]
        public void Instantiate_JobCompletedEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = new JobCompletedEvent
                {
                    JobId = Guid.NewGuid().ToString(),
                    ProcessedImages = 500,
                    FailedImages = 2,
                    DurationMs = 12500,
                    Success = true
                };
                _ = ev.EventId;
            }
        }

        [Benchmark]
        public void Instantiate_ImageProcessingCompletedEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = new ImageProcessingCompletedEvent
                {
                    ImageId = Guid.NewGuid().ToString(),
                    OutputPath = "/data/output/image_processed.png",
                    ProcessedSize = 2048000,
                    DurationMs = 45,
                    ProcessingMetrics = _metrics
                };
                _ = ev.EventId;
            }
        }

        [Benchmark]
        public void Instantiate_JobFailedEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = new JobFailedEvent
                {
                    JobId = Guid.NewGuid().ToString(),
                    ErrorMessage = "GPU memory overflow",
                    ErrorCode = "GPU_OOM",
                    AttemptNumber = 1,
                    Retryable = false
                };
                _ = ev.EventId;
            }
        }
    }
}
