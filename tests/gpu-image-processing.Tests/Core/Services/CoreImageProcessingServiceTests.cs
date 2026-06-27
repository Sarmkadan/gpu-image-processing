#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Core.Services;

public class CoreImageProcessingServiceTests
{
    private readonly Mock<ImageRepository> _imageRepositoryMock;
    private readonly Mock<GenericRepository<Filter>> _filterRepositoryMock;
    private readonly Mock<GenericRepository<Transform>> _transformRepositoryMock;
    private readonly Mock<GenericRepository<ProcessingProfile>> _profileRepositoryMock;
    private readonly Mock<DeviceService> _deviceServiceMock;
    private readonly Mock<IComputeShaderPipeline> _computeShaderPipelineMock;
    private readonly Mock<ILogger<ImageProcessingService>> _loggerMock;
    private readonly Mock<FilterService> _filterServiceMock;
    private readonly Mock<TransformService> _transformServiceMock;
    private readonly ImageProcessingService _sut;

    public CoreImageProcessingServiceTests()
    {
        _imageRepositoryMock = new Mock<ImageRepository>();
        _filterRepositoryMock = new Mock<GenericRepository<Filter>>();
        _transformRepositoryMock = new Mock<GenericRepository<Transform>>();
        _profileRepositoryMock = new Mock<GenericRepository<ProcessingProfile>>();
        _deviceServiceMock = new Mock<DeviceService>();
        _computeShaderPipelineMock = new Mock<IComputeShaderPipeline>();
        _loggerMock = new Mock<ILogger<ImageProcessingService>>();
        _filterServiceMock = new Mock<FilterService>();
        _transformServiceMock = new Mock<TransformService>();

        _sut = new ImageProcessingService(
            _imageRepositoryMock.Object,
            _filterRepositoryMock.Object,
            _transformRepositoryMock.Object,
            _profileRepositoryMock.Object,
            _deviceServiceMock.Object,
            _computeShaderPipelineMock.Object,
            _loggerMock.Object,
            _filterServiceMock.Object,
            _transformServiceMock.Object);
    }

    [Fact]
    public async Task RegisterImageAsync_ValidPath_ReturnsImage()
    {
        // Arrange
        var filePath = "test.png";
        var name = "test";
        _imageRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Image>())).ReturnsAsync((Image img) => img);

        // Act
        var result = await _sut.RegisterImageAsync(filePath, name);

        // Assert
        result.Should().NotBeNull();
        result.FilePath.Should().Be(filePath);
        result.Name.Should().Be(name);
    }

    [Fact]
    public async Task RegisterImageAsync_EmptyPath_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _sut.RegisterImageAsync("", "name");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetImageAsync_ExistingId_ReturnsImage()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var expectedImage = new Image();
        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId)).ReturnsAsync(expectedImage);

        // Act
        var result = await _sut.GetImageAsync(imageId);

        // Assert
        result.Should().Be(expectedImage);
    }

    [Fact]
    public async Task GetImageAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId)).ReturnsAsync((Image?)null);

        // Act
        var result = await _sut.GetImageAsync(imageId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CanProcessAsync_NoDevice_ReturnsFalse()
    {
        // Arrange
        _deviceServiceMock.Setup(x => x.GetSelectedDevice()).Returns((DeviceInfo?)null);

        // Act
        var result = await _sut.CanProcessAsync(new List<Guid>(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
