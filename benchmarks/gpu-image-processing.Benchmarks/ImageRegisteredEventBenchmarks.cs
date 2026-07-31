using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GpuImageProcessing.Benchmarks
{
    [MemoryDiagnoser]
    public class ImageRegisteredEventBenchmarks
    {
        // Parameter for the number of iterations in each benchmark
        [Params(10, 100, 1000)]
        public int N { get; set; }

        // Sample data used to populate the event
        private Guid _imageId;
        private string _imagePath;
        private int _width;
        private int _height;
        private string _description;

        // Serialized JSON representation of a sample event (used for deserialization benchmark)
        private string _jsonSample;

        [GlobalSetup]
        public void Setup()
        {
            _imageId = Guid.NewGuid();
            _imagePath = "/data/images/sample_image.png";
            _width = 1920;
            _height = 1080;
            _description = "Sample image for benchmarking";

            // Prepare a JSON string that matches the structure of ImageRegisteredEvent
            var sampleEvent = new ImageRegisteredEvent
            {
                ImageId = _imageId,
                ImagePath = _imagePath,
                Width = _width,
                Height = _height,
                Description = _description
            };
            _jsonSample = JsonSerializer.Serialize(sampleEvent);
        }

        // Benchmark: instantiate ImageRegisteredEvent objects
        [Benchmark]
        public void InstantiateEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = new ImageRegisteredEvent
                {
                    ImageId = Guid.NewGuid(),
                    ImagePath = _imagePath,
                    Width = _width,
                    Height = _height,
                    Description = _description
                };
                // Prevent dead‑code elimination
                GC.KeepAlive(ev);
            }
        }

        // Benchmark: serialize ImageRegisteredEvent to JSON
        [Benchmark]
        public void SerializeEvent()
        {
            var ev = new ImageRegisteredEvent
            {
                ImageId = _imageId,
                ImagePath = _imagePath,
                Width = _width,
                Height = _height,
                Description = _description
            };

            for (int i = 0; i < N; i++)
            {
                string json = JsonSerializer.Serialize(ev);
                // Prevent dead‑code elimination
                GC.KeepAlive(json);
            }
        }

        // Benchmark: deserialize JSON back to ImageRegisteredEvent
        [Benchmark]
        public void DeserializeEvent()
        {
            for (int i = 0; i < N; i++)
            {
                var ev = JsonSerializer.Deserialize<ImageRegisteredEvent>(_jsonSample);
                // Prevent dead‑code elimination
                GC.KeepAlive(ev);
            }
        }
    }
}
