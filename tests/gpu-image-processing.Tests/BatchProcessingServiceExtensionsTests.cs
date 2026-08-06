using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests
{
    public class BatchProcessingServiceExtensionsTests
    {
        private readonly Mock<BatchProcessingService> _serviceMock;

        public BatchProcessingServiceExtensionsTests()
        {
            // Create mocks for BatchProcessingService dependencies
            var processingServiceMock = new Mock<ImageProcessingService>(
                Mock.Of<ImageRepository>(),
                Mock.Of<FilterConfigurationRepository>(),
                Mock.Of<ProcessingResultRepository>(),
                Mock.Of<FilterService>(),
                Mock.Of<GpuManagementService>(),
                Mock.Of<PerformanceMonitoringService>(),
                Mock.Of<ILogger<ImageProcessingService>>());

            var imageRepositoryMock = new Mock<ImageRepository>();
            var loggerMock = new Mock<ILogger<BatchProcessingService>>();

            _serviceMock = new Mock<BatchProcessingService>(
                processingServiceMock.Object,
                imageRepositoryMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public void GetActiveBatchIds_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService service = null!;
            Assert.Throws<ArgumentNullException>(() => BatchProcessingServiceExtensions.GetActiveBatchIds(service));
        }

        [Fact]
        public void GetActiveBatchIds_NoActiveBatches_ReturnsEmptyList()
        {
            _serviceMock.Setup(s => s.GetActiveBatches()).Returns(Enumerable.Empty<ImageBatch>());

            var result = BatchProcessingServiceExtensions.GetActiveBatchIds(_serviceMock.Object);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetActiveBatchIds_WithActiveBatches_ReturnsBatchIds()
        {
            var batch1 = new ImageBatch { Id = Guid.NewGuid(), Name = "Batch 1" };
            var batch2 = new ImageBatch { Id = Guid.NewGuid(), Name = "Batch 2" };
            var batches = new List<ImageBatch> { batch1, batch2 };
            _serviceMock.Setup(s => s.GetActiveBatches()).Returns(batches);

            var result = BatchProcessingServiceExtensions.GetActiveBatchIds(_serviceMock.Object);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(batch1.Id, result);
            Assert.Contains(batch2.Id, result);
        }

        [Fact]
        public void GetAllActiveBatchProgress_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService service = null!;
            Assert.Throws<ArgumentNullException>(() => BatchProcessingServiceExtensions.GetAllActiveBatchProgress(service));
        }

        [Fact]
        public void GetAllActiveBatchProgress_WithActiveBatches_ReturnsProgressDictionary()
        {
            var batchId1 = Guid.NewGuid();
            var batchId2 = Guid.NewGuid();
            var batch1 = new ImageBatch { Id = batchId1, Name = "Batch 1" };
            var batch2 = new ImageBatch { Id = batchId2, Name = "Batch 2" };
            var batches = new List<ImageBatch> { batch1, batch2 };
            _serviceMock.Setup(s => s.GetActiveBatches()).Returns(batches);

            _serviceMock.Setup(s => s.GetBatchProgress(batchId1)).Returns(new Dictionary<string, object> { { "Status", "Processing" } });
            _serviceMock.Setup(s => s.GetBatchProgress(batchId2)).Returns(new Dictionary<string, object> { { "Status", "Completed" } });

            var result = BatchProcessingServiceExtensions.GetAllActiveBatchProgress(_serviceMock.Object);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(batchId1));
            Assert.True(result.ContainsKey(batchId2));
            Assert.Equal("Processing", result[batchId1]["Status"]);
            Assert.Equal("Completed", result[batchId2]["Status"]);
        }

        [Fact]
        public void GetBatchSummary_NullService_ThrowsArgumentNullException()
        {
            BatchProcessingService service = null!;
            Assert.Throws<ArgumentNullException>(() => BatchProcessingServiceExtensions.GetBatchSummary(service, Guid.NewGuid()));
        }

        [Fact]
        public void GetBatchSummary_BatchNotFound_ReturnsNotFoundMessage()
        {
            var batchId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetBatchStatus(batchId)).Returns((ImageBatch)null!);

            var result = BatchProcessingServiceExtensions.GetBatchSummary(_serviceMock.Object, batchId);

            Assert.Equal($"Batch {batchId} not found.", result);
        }

        [Fact]
        public void GetBatchSummary_BatchFound_ReturnsFormattedSummary()
        {
            var batchId = Guid.NewGuid();
            var batch = new ImageBatch
            {
                Id = batchId,
                Status = ProcessingStatus.Processing,
                ProcessedImages = 5,
                TotalImages = 10,
                FailedImages = 2
            };
            _serviceMock.Setup(s => s.GetBatchStatus(batchId)).Returns(batch);

            var result = BatchProcessingServiceExtensions.GetBatchSummary(_serviceMock.Object, batchId);

            Assert.Contains($"Batch {batchId}", result);
            Assert.Contains("Status=Processing", result);
            Assert.Contains("Processed=5/10", result);
            Assert.Contains("Failed=2", result);
            Assert.Contains("Progress=50.00%", result);
        }
    }
}