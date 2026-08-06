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
    public class FilterConfigurationRepositoryExtensionsTests
    {
        private FilterConfiguration CreateTestFilter(string name, FilterType type, bool isActive = true)
        {
            return new FilterConfiguration
            {
                Name = name,
                FilterType = type,
                IsActive = isActive,
                Priority = 1
            };
        }

        [Fact]
        public async Task GetActiveByTypeAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            FilterConfigurationRepository repository = null!;
            FilterType type = FilterType.Grayscale;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetActiveByTypeAsync(repository, type));
        }

        [Fact]
        public async Task GetActiveByTypeAsync_NoActiveFilters_ReturnsNull()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();
            var filter = CreateTestFilter("Test Filter", FilterType.Grayscale, false); // Not active
            await repository.CreateAsync(filter);

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetActiveByTypeAsync(
                repository, FilterType.Grayscale);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveByTypeAsync_ActiveFilterExists_ReturnsFilter()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();
            var filter = CreateTestFilter("Test Filter", FilterType.Grayscale, true);
            await repository.CreateAsync(filter);

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetActiveByTypeAsync(
                repository, FilterType.Grayscale);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FilterType.Grayscale, result?.FilterType);
            Assert.Equal("Test Filter", result?.Name);
        }

        [Fact]
        public async Task GetByNamePatternAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            FilterConfigurationRepository repository = null!;
            string namePattern = "test";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetByNamePatternAsync(repository, namePattern));
        }

        [Fact]
        public async Task GetByNamePatternAsync_NullNamePattern_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetByNamePatternAsync(repository, null!));
        }

        [Fact]
        public async Task GetByNamePatternAsync_EmptyNamePattern_ReturnsEmptyList()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetByNamePatternAsync(
                repository, string.Empty);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByNamePatternAsync_WhitespaceNamePattern_ReturnsEmptyList()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetByNamePatternAsync(
                repository, "   ");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByNamePatternAsync_MatchingFilters_ReturnsMatchingFilters()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();
            var filter1 = CreateTestFilter("Grayscale Filter", FilterType.Grayscale);
            var filter2 = CreateTestFilter("Blur Filter", FilterType.Blur);
            var filter3 = CreateTestFilter("Gray Scale Advanced", FilterType.Grayscale);
            await repository.CreateAsync(filter1);
            await repository.CreateAsync(filter2);
            await repository.CreateAsync(filter3);

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetByNamePatternAsync(
                repository, "gray");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, f => Assert.Contains("gray", f.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetByTypesAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            FilterConfigurationRepository repository = null!;
            var types = new List<FilterType> { FilterType.Grayscale };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetByTypesAsync(repository, types));
        }

        [Fact]
        public async Task GetByTypesAsync_NullTypes_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetByTypesAsync(repository, null!));
        }

        [Fact]
        public async Task GetByTypesAsync_EmptyTypes_ReturnsEmptyList()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetByTypesAsync(
                repository, Enumerable.Empty<FilterType>());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTypesAsync_WithTypes_ReturnsMatchingFilters()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();
            var filter1 = CreateTestFilter("Grayscale Filter", FilterType.Grayscale);
            var filter2 = CreateTestFilter("Blur Filter", FilterType.Blur);
            var filter3 = CreateTestFilter("Sharpen Filter", FilterType.Sharpen);
            await repository.CreateAsync(filter1);
            await repository.CreateAsync(filter2);
            await repository.CreateAsync(filter3);

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetByTypesAsync(
                repository, new List<FilterType> { FilterType.Grayscale, FilterType.Sharpen });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, f => Assert.Contains(f.FilterType, new[] { FilterType.Grayscale, FilterType.Sharpen }));
        }

        [Fact]
        public async Task GetActivePagedAsync_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            FilterConfigurationRepository repository = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                FilterConfigurationRepositoryExtensions.GetActivePagedAsync(repository, 1, 10));
        }

        [Fact]
        public async Task GetActivePagedAsync_InvalidPageNumber_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                FilterConfigurationRepositoryExtensions.GetActivePagedAsync(repository, 0, 10));
        }

        [Fact]
        public async Task GetActivePagedAsync_InvalidPageSize_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                FilterConfigurationRepositoryExtensions.GetActivePagedAsync(repository, 1, 0));
        }

        [Fact]
        public async Task GetActivePagedAsync_NoFilters_ReturnsEmptyList()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();

            // Act
            var result = await FilterConfigurationRepositoryExtensions.GetActivePagedAsync(
                repository, 1, 10);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetActivePagedAsync_WithFilters_ReturnsPagedResults()
        {
            // Arrange
            var repository = new FilterConfigurationRepository();
            // Create 15 active filters
            for (int i = 0; i < 15; i++)
            {
                var filter = CreateTestFilter($"Filter {i}", FilterType.Grayscale);
                await repository.CreateAsync(filter);
            }

            // Act - Get first page (page 1, size 5)
            var result = await FilterConfigurationRepositoryExtensions.GetActivePagedAsync(
                repository, 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
            // Should be first 5 filters (ordered by priority, which is all 1, so by insertion order)
            Assert.All(result.Select((f, index) => new { f, index }), item =>
                Assert.Equal($"Filter {item.index}", item.f.Name));
        }
    }
}