using System;
using System.Collections.Generic;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests
{
    public class FilterChainValidationTests
    {
        private FilterChain CreateValidFilterChain()
        {
            var chain = new FilterChain
            {
                Name = "Valid Chain",
                Description = "A valid filter chain for testing",
                IsEnabled = true,
                ExecutionOrder = 0,
                AllowParallelExecution = false,
                MaxParallelSteps = 0,
                CacheIntermediateResults = false
            };
            chain.CreatedAt = DateTime.UtcNow;
            chain.ModifiedAt = DateTime.UtcNow;
            chain.ChainOptions = new Dictionary<string, object> { { "key1", "value1" } };
            chain.AddStep(Guid.NewGuid());
            return chain;
        }

        [Fact]
        public void Validate_ValidChain_ReturnsEmptyProblems()
        {
            // Arrange
            var chain = CreateValidFilterChain();

            // Act
            var problems = FilterChainValidation.Validate(chain);

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void IsValid_ValidChain_ReturnsTrue()
        {
            // Arrange
            var chain = CreateValidFilterChain();

            // Act
            bool isValid = FilterChainValidation.IsValid(chain);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void EnsureValid_ValidChain_DoesNotThrow()
        {
            // Arrange
            var chain = CreateValidFilterChain();

            // Act
            var exception = Record.Exception(() => FilterChainValidation.EnsureValid(chain));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullChain_ThrowsArgumentNullException()
        {
            // Act
            var exception = Record.Exception(() => FilterChainValidation.Validate(null!));

            // Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void IsValid_NullChain_ThrowsArgumentNullException()
        {
            // Act
            var exception = Record.Exception(() => FilterChainValidation.IsValid(null!));

            // Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void EnsureValid_NullChain_ThrowsArgumentNullException()
        {
            // Act
            var exception = Record.Exception(() => FilterChainValidation.EnsureValid(null!));

            // Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Validate_EmptySteps_ReturnsProblemAboutEmptySteps()
        {
            // Arrange
            var chain = new FilterChain
            {
                Name = "Valid Chain",
                Description = "A valid filter chain for testing",
                IsEnabled = true,
                ExecutionOrder = 0,
                AllowParallelExecution = false,
                MaxParallelSteps = 0,
                CacheIntermediateResults = false
            };
            chain.CreatedAt = DateTime.UtcNow;
            chain.ModifiedAt = DateTime.UtcNow;
            chain.ChainOptions = new Dictionary<string, object> { { "key1", "value1" } };
            // Steps is empty by default

            // Act
            var problems = FilterChainValidation.Validate(chain);

            // Assert
            Assert.Contains(problems, p => p.Contains("FilterChain.Steps cannot be empty"));
        }

        [Fact]
        public void Validate_StepWithEmptyFilterId_ReturnsProblemAboutFilterId()
        {
            // Arrange
            var chain = CreateValidFilterChain();
            // Replace the first step's FilterId with Empty
            chain.Steps[0].FilterId = Guid.Empty;

            // Act
            var problems = FilterChainValidation.Validate(chain);

            // Assert
            Assert.Contains(problems, p => p.Contains("FilterChain.Steps[0].FilterId cannot be empty"));
        }
    }
}