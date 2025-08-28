#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Contains unit tests for the <see cref="FilterService"/> class.
/// </summary>
public class FilterServiceTests
{
    private readonly FilterConfigurationRepository _repository;
    private readonly Mock<ILogger<FilterService>> _loggerMock;
    private readonly FilterService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterServiceTests"/> class, setting up the repository and logger mock.
    /// </summary>
    public FilterServiceTests()
    {
        _repository = new FilterConfigurationRepository();
        _loggerMock = new Mock<ILogger<FilterService>>();
        _sut = new FilterService(_repository, _loggerMock.Object);
    }

    private static Image CreateValidImage(ColorSpace colorSpace = ColorSpace.Rgb) => new()
    {
        FilePath = "/tmp/test.jpg",
        FileName = "test.jpg",
        Width = 800,
        Height = 600,
        BitsPerPixel = 24,
        FileSizeBytes = 1_000_000,
        ColorSpace = colorSpace
    };

    private static FilterConfiguration CreateFilterConfig(
        string name, FilterType type, bool isActive = true) => new()
    {
        Name = name,
        FilterType = type,
        IsActive = isActive,
        MaxThreadsPerBlock = 256
    };

    /// <summary>
    /// Verifies that <see cref="FilterService.ApplyFilterAsync"/> throws an <see cref="InvalidFilterException"/> when applying a filter that does not exist.
    /// </summary>
    [Fact]
    public async Task ApplyFilterAsync_FilterNotFound_ThrowsInvalidFilterException()
    {
        // Arrange
        var image = CreateValidImage();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _sut.ApplyFilterAsync(image, nonExistentId);

        // Assert
        await act.Should().ThrowAsync<InvalidFilterException>();
    }

    /// <summary>
    /// Verifies that <see cref="FilterService.ApplyFilterAsync"/> throws an <see cref="InvalidFilterException"/> when the requested filter is inactive.
    /// </summary>
    [Fact]
    public async Task ApplyFilterAsync_InactiveFilter_ThrowsInvalidFilterException()
    {
        // Arrange
        var image = CreateValidImage();
        var config = CreateFilterConfig("Inactive Blur", FilterType.Blur, isActive: false);
        var saved = await _repository.CreateAsync(config);

        // Act
        var act = async () => await _sut.ApplyFilterAsync(image, saved.Id);

        // Assert
        var assertion = await act.Should().ThrowAsync<InvalidFilterException>();
        assertion.Which.Message.Should().Contain("not active");
    }

    /// <summary>
    /// Verifies that <see cref="FilterService.ApplyFilterAsync"/> successfully applies the Grayscale filter, updating the image's color space and bits per pixel.
    /// </summary>
    [Fact]
    public async Task ApplyFilterAsync_GrayscaleFilter_SetsColorSpaceToGrayscale()
    {
        // Arrange
        var image = CreateValidImage(ColorSpace.Rgb);
        var config = CreateFilterConfig("Grayscale", FilterType.Grayscale);
        var saved = await _repository.CreateAsync(config);

        // Act
        var result = await _sut.ApplyFilterAsync(image, saved.Id);

        // Assert
        result.ColorSpace.Should().Be(ColorSpace.Grayscale);
        result.BitsPerPixel.Should().Be(8);
    }

    /// <summary>
    /// Verifies that <see cref="FilterService.CreateFilterAsync"/> throws an <see cref="ArgumentNullException"/> when the configuration is null.
    /// </summary>
    [Fact]
    public async Task CreateFilterAsync_NullConfig_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _sut.CreateFilterAsync(null!);

        // Assert
        var assertion = await act.Should().ThrowAsync<ArgumentNullException>();
        assertion.Which.ParamName.Should().Be("config");
    }

    /// <summary>
    /// Verifies that <see cref="FilterService.CreateFilterAsync"/> throws an <see cref="InvalidFilterException"/> when providing an invalid configuration.
    /// </summary>
    [Fact]
    public async Task CreateFilterAsync_InvalidConfig_ThrowsInvalidFilterException()
    {
        // Arrange — FilterType.None always fails Validate()
        var config = new FilterConfiguration
        {
            Name = "Bad Filter",
            FilterType = FilterType.None,
            MaxThreadsPerBlock = 256
        };

        // Act
        var act = async () => await _sut.CreateFilterAsync(config);

        // Assert
        await act.Should().ThrowAsync<InvalidFilterException>();
    }

    /// <summary>
    /// Verifies that <see cref="FilterService.GetFiltersByTypeAsync"/> returns only filters of the specified <see cref="FilterType"/> when multiple types are stored.
    /// </summary>
    [Fact]
    public async Task GetFiltersByTypeAsync_MultipleTypesStored_ReturnsOnlyMatchingType()
    {
        // Arrange
        await _repository.CreateAsync(CreateFilterConfig("Blur A", FilterType.Blur));
        await _repository.CreateAsync(CreateFilterConfig("Sharpen A", FilterType.Sharpen));
        await _repository.CreateAsync(CreateFilterConfig("Grayscale A", FilterType.Grayscale));

        // Act
        var results = await _sut.GetFiltersByTypeAsync(FilterType.Blur);

        // Assert
        results.Should().HaveCount(1);
        results.Should().OnlyContain(f => f.FilterType == FilterType.Blur);
    }
}
