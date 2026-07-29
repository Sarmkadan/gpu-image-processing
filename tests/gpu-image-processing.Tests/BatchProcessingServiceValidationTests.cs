using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using GpuImageProcessing.Services;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Tests.Services
{
    public class BatchProcessingServiceValidationTests
    {
        private BatchProcessingService CreateService(
            ImageProcessingService? processingService = null,
            ImageRepository? repository = null,
            ILogger<BatchProcessingService>? logger = null)
        {
            // Provide mock instances for any null arguments
            var procService = processingService ?? new Mock<ImageProcessingService>().Object;
            var repo = repository ?? new Mock<ImageRepository>().Object;
            var log = logger ?? NullLogger<BatchProcessingService>.Instance;

            return new BatchProcessingService(procService, repo, log);
        }

        [Fact]
        public void Validate_WithValidService_ReturnsEmptyList()
        {
            var service = CreateService();
            IReadOnlyList<string> result = service.Validate();

            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_WithValidService_ReturnsTrue()
        {
            var service = CreateService();
            bool isValid = service.IsValid();

            Assert.True(isValid);
        }

        [Fact]
        public void EnsureValid_WithValidService_DoesNotThrow()
        {
            var service = CreateService();
            var exception = Record.Exception(() => service.EnsureValid());

            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService? service = null;
            Assert.Throws<ArgumentNullException>(() => service!.Validate());
        }

        [Fact]
        public void IsValid_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService? service = null;
            Assert.Throws<ArgumentNullException>(() => service!.IsValid());
        }

        [Fact]
        public void EnsureValid_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService? service = null;
            Assert.Throws<ArgumentNullException>(() => service!.EnsureValid());
        }

        [Fact]
        public void Validate_WithNullDependency_ReturnsProblem()
        {
            // Pass a null repository to provoke a NullReferenceException inside GetActiveBatchCount
            var service = new BatchProcessingService(
                new Mock<ImageProcessingService>().Object,
                null!,
                NullLogger<BatchProcessingService>.Instance);

            IReadOnlyList<string> result = service.Validate();

            Assert.Single(result);
            Assert.Contains("Service dependencies contain null references", result[0]);
        }
    }
}
