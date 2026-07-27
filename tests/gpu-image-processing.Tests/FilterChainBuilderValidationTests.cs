using System;
using System.Collections.Generic;
using System.Reflection;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests
{
    public class FilterChainBuilderValidationTests
    {
        private static FilterChainBuilder CreateBuilder(
            string? name = null,
            bool allowParallel = false,
            int maxParallelSteps = 0,
            IEnumerable<(Guid FilterId, double EstimatedMs)>? steps = null)
        {
            // The builder likely does not have a public ctor, so create it via reflection.
            var builder = (FilterChainBuilder)Activator.CreateInstance(
                typeof(FilterChainBuilder),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: null,
                culture: null)!;

            // Set private fields that the validation logic reads.
            var type = typeof(FilterChainBuilder);
            type.GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(builder, name);
            type.GetField("_allowParallel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(builder, allowParallel);
            type.GetField("_maxParallelSteps", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(builder, maxParallelSteps);

            // _steps is a List<(Guid, double)>
            var stepsList = new List<(Guid FilterId, double EstimatedMs)>();
            if (steps != null)
            {
                stepsList.AddRange(steps);
            }
            type.GetField("_steps", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(builder, stepsList);

            return builder;
        }

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyAndIsValid()
        {
            // Arrange: a minimal valid builder
            var builder = CreateBuilder(
                name: "valid-chain",
                allowParallel: false,
                maxParallelSteps: 0,
                steps: new[] { (Guid.NewGuid(), 12.5) });

            // Act
            var errors = builder.Validate();
            var isValid = builder.IsValid();

            // Assert
            Assert.Empty(errors);
            Assert.True(isValid);
            // EnsureValid should not throw
            var exception = Record.Exception(() => builder.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullBuilder_ThrowsArgumentNullException()
        {
            // Validate
            Assert.Throws<ArgumentNullException>(() => ((FilterChainBuilder)null!).Validate());

            // IsValid
            Assert.Throws<ArgumentNullException>(() => ((FilterChainBuilder)null!).IsValid());

            // EnsureValid
            Assert.Throws<ArgumentNullException>(() => ((FilterChainBuilder)null!).EnsureValid());
        }

        [Fact]
        public void Validate_BlankName_ReturnsError()
        {
            // Arrange: name is null / whitespace
            var builder = CreateBuilder(
                name: "   ",
                steps: new[] { (Guid.NewGuid(), 5.0) });

            // Act
            var errors = builder.Validate();

            // Assert
            Assert.Contains("Chain name must not be blank.", errors);
            Assert.False(builder.IsValid());
            Assert.Throws<ArgumentException>(() => builder.EnsureValid());
        }

        [Fact]
        public void Validate_ParallelEnabled_InvalidMaxSteps_ReturnsError()
        {
            // Arrange: parallel enabled but max steps is 0 (invalid)
            var builder = CreateBuilder(
                name: "parallel-chain",
                allowParallel: true,
                maxParallelSteps: 0,
                steps: new[] { (Guid.NewGuid(), 3.0) });

            // Act
            var errors = builder.Validate();

            // Assert
            Assert.Contains("must be at least 1", errors[0]);
            Assert.False(builder.IsValid());
            var ex = Assert.Throws<ArgumentException>(() => builder.EnsureValid());
            Assert.Contains("must be at least 1", ex.Message);
        }

        [Fact]
        public void Validate_NoSteps_ReturnsError()
        {
            // Arrange: valid name but no steps added
            var builder = CreateBuilder(
                name: "no-steps",
                steps: Array.Empty<(Guid, double)>());

            // Act
            var errors = builder.Validate();

            // Assert
            Assert.Contains("must contain at least one filter step", errors[0]);
            Assert.False(builder.IsValid());
            var ex = Assert.Throws<ArgumentException>(() => builder.EnsureValid());
            Assert.Contains("must contain at least one filter step", ex.Message);
        }

        [Fact]
        public void Validate_ParallelDisabled_MaxStepsGreaterThanOne_ReturnsError()
        {
            // Arrange: parallel disabled but maxParallelSteps set to 2 (invalid)
            var builder = CreateBuilder(
                name: "serial-chain",
                allowParallel: false,
                maxParallelSteps: 2,
                steps: new[] { (Guid.NewGuid(), 1.0) });

            // Act
            var errors = builder.Validate();

            // Assert
            Assert.Contains("must be 0 or 1", errors[0]);
            Assert.False(builder.IsValid());
        }
    }
}
