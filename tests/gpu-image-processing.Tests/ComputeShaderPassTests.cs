using Xunit;
using GpuImageProcessing.Domain;
using System;

namespace GpuImageProcessing.Tests
{
    public class ComputeShaderPassTests
    {
        [Fact]
        public void Constructor_HappyPath_SetsProperties()
        {
            // Arrange
            var kernelName = "TestKernel";
            var kernelSource = "TestSource";
            var passType = ShaderPassType.ImageFilter;
            var priority = 1;

            // Act
            var pass = new ComputeShaderPass(kernelName, kernelSource, passType, priority);

            // Assert
            Assert.NotEqual(Guid.Empty, pass.Id);
            Assert.Equal(kernelName, pass.KernelName);
            Assert.Equal(kernelSource, pass.KernelSource);
            Assert.Equal(passType, pass.PassType);
            Assert.Equal(priority, pass.Priority);
            Assert.Empty(pass.Parameters);
            Assert.Empty(pass.InputImages);
            Assert.Null(pass.OutputImage);
        }

        [Fact]
        public void Constructor_NullKernelName_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new ComputeShaderPass(null));
        }

        [Fact]
        public void Constructor_WhitespaceKernelName_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new ComputeShaderPass("   "));
        }

        [Fact]
        public void Properties_Getters_ReturnExpectedValues()
        {
            // Arrange
            var pass = new ComputeShaderPass("TestKernel");

            // Act and Assert
            Assert.NotEqual(Guid.Empty, pass.Id);
            Assert.Equal("TestKernel", pass.KernelName);
            Assert.Equal("", pass.KernelSource);
            Assert.Equal(ShaderPassType.ImageFilter, pass.PassType);
            Assert.Equal(0, pass.Priority);
            Assert.Empty(pass.Parameters);
            Assert.Empty(pass.InputImages);
            Assert.Null(pass.OutputImage);
        }

        [Fact]
        public void WorkgroupConfiguration_Setter_SetsValue()
        {
            // Arrange
            var pass = new ComputeShaderPass("TestKernel");
            var workgroupConfiguration = new WorkgroupConfiguration();

            // Act
            pass.WorkgroupConfiguration = workgroupConfiguration;

            // Assert
            Assert.Equal(workgroupConfiguration, pass.WorkgroupConfiguration);
        }

        [Fact]
        public void OutputImage_Setter_SetsValue()
        {
            // Arrange
            var pass = new ComputeShaderPass("TestKernel");
            var outputImage = new Image();

            // Act
            pass.OutputImage = outputImage;

            // Assert
            Assert.Equal(outputImage, pass.OutputImage);
        }
    }
}
