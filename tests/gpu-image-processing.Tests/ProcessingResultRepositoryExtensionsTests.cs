#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Core;

namespace gpu_image_processing.Tests
{
    public class ProcessingResultRepositoryExtensionsTests
    {
        private ProcessingResult CreateTestResult(Guid imageId, ProcessingStatus status, long processingTimeMs, bool isSuccessful = true)
        {
            var result = new ProcessingResult
            {
                ImageId = imageId,
                Status = status,
                ProcessingTimeMilliseconds = processingTimeMs,
                IsSuccessful = isSuccessful
            };
            if (isSuccessful)
            {
                result.CompletedAt = DateTime.UtcNow;
                result.StartedAt = result.CompletedAt.AddMilliseconds(-processingTimeMs);
            }
            else
            {
                result.CompletedAt = DateTime.UtcNow;
                result.StartedAt = result.CompletedAt.AddMilliseconds(-processingTimeMs);
            }
            return result;
        }

        [Fact]
        public async Task GetByImageIdsAsync_ValidInput_ReturnsResults()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId1 = Guid.NewGuid();
            var imageId2 = Guid.NewGuid();
            var result1 = CreateTestResult(imageId1, ProcessingStatus.Completed, 100);
            var result2 = CreateTestResult(imageId2, ProcessingStatus.Completed, 200);
            await repository.CreateAsync(result1);
            await repository.CreateAsync(result2);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetByImageIdsAsync(
                repository, new[] { imageId1, imageId2 });

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.ImageId == imageId1);
            Assert.Contains(results, r => r.ImageId == imageId2);
        }

        [Fact]
        public async Task GetByImageIdsAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResultRepository repository = null!;
            var imageIds = new List<Guid> { Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetByImageIdsAsync(repository, imageIds));
        }

        [Fact]
        public async Task GetByImageIdsAsync_NullImageIds_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            IEnumerable<Guid> imageIds = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetByImageIdsAsync(repository, imageIds));
        }

        [Fact]
        public async Task GetByImageIdsAsync_EmptyImageIds_ReturnsEmptyList()
        {
            // Arrange
            var repository = new ProcessingResultRepository();

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetByImageIdsAsync(
                repository, Enumerable.Empty<Guid>());

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetByStatusesAsync_ValidInput_ReturnsResults()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId = Guid.NewGuid();
            var result1 = CreateTestResult(imageId, ProcessingStatus.Completed, 100);
            var result2 = CreateTestResult(imageId, ProcessingStatus.Failed, 50, false);
            await repository.CreateAsync(result1);
            await repository.CreateAsync(result2);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetByStatusesAsync(
                repository, new[] { ProcessingStatus.Completed, ProcessingStatus.Failed });

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.Status == ProcessingStatus.Completed);
            Assert.Contains(results, r => r.Status == ProcessingStatus.Failed);
        }

        [Fact]
        public async Task GetByStatusesAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResultRepository repository = null!;
            var statuses = new List<ProcessingStatus> { ProcessingStatus.Completed };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetByStatusesAsync(repository, statuses));
        }

        [Fact]
        public async Task GetByStatusesAsync_NullStatuses_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            IEnumerable<ProcessingStatus> statuses = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetByStatusesAsync(repository, statuses));
        }

        [Fact]
        public async Task GetByStatusesAsync_EmptyStatuses_ReturnsEmptyList()
        {
            // Arrange
            var repository = new ProcessingResultRepository();

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetByStatusesAsync(
                repository, Enumerable.Empty<ProcessingStatus>());

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetRecentlyCompletedAsync_ValidInput_ReturnsResults()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId = Guid.NewGuid();
            var result = CreateTestResult(imageId, ProcessingStatus.Completed, 100);
            // Set CompletedAt to now - 12 hours (within last 2 days)
            result.CompletedAt = DateTime.UtcNow.AddHours(-12);
            result.StartedAt = result.CompletedAt.AddMilliseconds(-result.ProcessingTimeMilliseconds);
            await repository.CreateAsync(result);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetRecentlyCompletedAsync(
                repository, 2); // last 2 days

            // Assert
            Assert.Single(results);
            Assert.Equal(result.Id, results.First().Id);
        }

        [Fact]
        public async Task GetRecentlyCompletedAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResultRepository repository = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetRecentlyCompletedAsync(repository, 1));
        }

        [Fact]
        public async Task GetRecentlyCompletedAsync_DaysLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var repository = new ProcessingResultRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                ProcessingResultRepositoryExtensions.GetRecentlyCompletedAsync(repository, 0));
        }

        [Fact]
        public async Task GetRecentlyCompletedAsync_NoMatchingResults_ReturnsEmptyList()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId = Guid.NewGuid();
            var result = CreateTestResult(imageId, ProcessingStatus.Completed, 100);
            // Set CompletedAt to 10 days ago
            result.CompletedAt = DateTime.UtcNow.AddDays(-10);
            result.StartedAt = result.CompletedAt.AddMilliseconds(-result.ProcessingTimeMilliseconds);
            await repository.CreateAsync(result);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetRecentlyCompletedAsync(
                repository, 2); // last 2 days

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetLongestRunningResultsAsync_ValidInput_ReturnsResults()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId1 = Guid.NewGuid();
            var imageId2 = Guid.NewGuid();
            var imageId3 = Guid.NewGuid();
            var result1 = CreateTestResult(imageId1, ProcessingStatus.Completed, 100);
            var result2 = CreateTestResult(imageId2, ProcessingStatus.Completed, 300);
            var result3 = CreateTestResult(imageId3, ProcessingStatus.Completed, 200);
            // Add an unsuccessful result that should be filtered out
            var result4 = CreateTestResult(Guid.NewGuid(), ProcessingStatus.Failed, 500, false);
            await repository.CreateAsync(result1);
            await repository.CreateAsync(result2);
            await repository.CreateAsync(result3);
            await repository.CreateAsync(result4);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetLongestRunningResultsAsync(
                repository, 2, 150); // top 2, minimum 150ms

            // Assert
            Assert.Equal(2, results.Count);
            // Should be result2 (300ms) and result3 (200ms) - sorted descending
            Assert.Equal(300, results[0].ProcessingTimeMilliseconds);
            Assert.Equal(200, results[1].ProcessingTimeMilliseconds);
            // Should not include the failed result or the one below minimum
            Assert.DoesNotContain(results, r => r.ImageId == imageId1); // 100ms < 150ms
            Assert.DoesNotContain(results, r => r.ImageId == imageId3 && r.ProcessingTimeMilliseconds == 500); // failed
        }

        [Fact]
        public async Task GetLongestRunningResultsAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResultRepository repository = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ProcessingResultRepositoryExtensions.GetLongestRunningResultsAsync(repository));
        }

        [Fact]
        public async Task GetLongestRunningResultsAsync_CountLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var repository = new ProcessingResultRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                ProcessingResultRepositoryExtensions.GetLongestRunningResultsAsync(repository, 0));
        }

        [Fact]
        public async Task GetLongestRunningResultsAsync_MinimumProcessingTimeMsLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var repository = new ProcessingResultRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                ProcessingResultRepositoryExtensions.GetLongestRunningResultsAsync(repository, 10, -1));
        }

        [Fact]
        public async Task GetLongestRunningResultsAsync_NoSuccessfulResults_ReturnsEmptyList()
        {
            // Arrange
            var repository = new ProcessingResultRepository();
            var imageId = Guid.NewGuid();
            var result = CreateTestResult(imageId, ProcessingStatus.Failed, 100, false);
            await repository.CreateAsync(result);

            // Act
            var results = await ProcessingResultRepositoryExtensions.GetLongestRunningResultsAsync(
                repository, 10, 0);

            // Assert
            Assert.Empty(results);
        }
    }
}