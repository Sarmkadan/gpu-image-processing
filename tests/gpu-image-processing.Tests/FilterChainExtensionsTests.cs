// tests/gpu-image-processing.Tests/FilterChainExtensionsTests.cs
using Xunit;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests
{
    public class FilterChainExtensionsTests
    {
        [Fact]
        public void FindStepByFilterId_HappyPath_ReturnsStep()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterStep = new FilterStep { FilterId = Guid.NewGuid() };
            filterChain.Steps.Add(filterStep);

            // Act
            var result = FilterChainExtensions.FindStepByFilterId(filterChain, filterStep.FilterId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(filterStep, result);
        }

        [Fact]
        public void FindStepByFilterId_NullFilterChain_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => FilterChainExtensions.FindStepByFilterId(null, Guid.NewGuid()));
        }

        [Fact]
        public void FindStepByFilterId_FilterIdNotFound_ReturnsNull()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterId = Guid.NewGuid();

            // Act
            var result = FilterChainExtensions.FindStepByFilterId(filterChain, filterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStepByIndex_HappyPath_ReturnsStep()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterStep = new FilterStep { FilterId = Guid.NewGuid() };
            filterChain.Steps.Add(filterStep);

            // Act
            var result = FilterChainExtensions.GetStepByIndex(filterChain, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(filterStep, result);
        }

        [Fact]
        public void GetStepByIndex_IndexOutOfBounds_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterStep = new FilterStep { FilterId = Guid.NewGuid() };
            filterChain.Steps.Add(filterStep);

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => FilterChainExtensions.GetStepByIndex(filterChain, 1));
        }

        [Fact]
        public void HasStep_HappyPath_ReturnsTrue()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterStep = new FilterStep { FilterId = Guid.NewGuid() };
            filterChain.Steps.Add(filterStep);

            // Act
            var result = FilterChainExtensions.HasStep(filterChain, filterStep.FilterId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasStep_FilterIdNotFound_ReturnsFalse()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterId = Guid.NewGuid();

            // Act
            var result = FilterChainExtensions.HasStep(filterChain, filterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CalculateMemoryFootprint_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var filterChain = new FilterChain();
            var filterStep = new FilterStep { FilterId = Guid.NewGuid() };
            filterChain.Steps.Add(filterStep);

            // Act
            var result = FilterChainExtensions.CalculateMemoryFootprint(filterChain);

            // Assert
            Assert.NotEqual(0, result);
        }

        [Fact]
        public void CalculateMemoryFootprint_NullFilterChain_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => FilterChainExtensions.CalculateMemoryFootprint(null));
        }
    }
}
