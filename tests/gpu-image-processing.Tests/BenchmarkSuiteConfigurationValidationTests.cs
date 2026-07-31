using GpuImageProcessing.Benchmarking;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading;

namespace GpuImageProcessing.Tests
{
    public class BenchmarkSuiteConfigurationValidationTests
    {
        [Fact]
        public void Validate_Happy_PATH_RunName_Provided()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                RunName = "ValidRunName"
            };

            // Act
            var errors = BenchmarkSuiteConfigurationValidation.Validate(config);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_HAPPY_PATH_OutputDirectory_Provided()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                OutputDirectory = "ValidOutputDirectory"
            };

            // Act
            var errors = BenchmarkSuiteConfigurationValidation.Validate(config);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_HAPPY_PATH_AccuracyLevel_Provided()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                AccuracyLevel = BenchmarkAccuracyLevel.Standard
            };

            // Act
            var errors = BenchmarkSuiteConfigurationValidation.Validate(config);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_NULL_Config_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => BenchmarkSuiteConfigurationValidation.Validate(null));
        }

        [Fact]
        public void Validate_Empty_RunName_ThrowsArgumentException()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                RunName = ""
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => BenchmarkSuiteConfigurationValidation.EnsureValid(config));
        }

        [Fact]
        public void Validate_Invalid_OutputDirectory_ThrowsArgumentException()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                OutputDirectory = "Invalid:Output:Directory"
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => BenchmarkSuiteConfigurationValidation.EnsureValid(config));
        }

        [Fact]
        public void Validate_Invalid_AccuracyLevel_ThrowsArgumentException()
        {
            // Arrange
            var config = new BenchmarkSuiteConfiguration
            {
                AccuracyLevel = (BenchmarkAccuracyLevel)100
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => BenchmarkSuiteConfigurationValidation.EnsureValid(config));
        }
    }
}
