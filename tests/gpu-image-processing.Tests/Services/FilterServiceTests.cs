#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GpuImageProcessing.Tests.Services;

public class FilterServiceTests
{
    private readonly FilterConfigurationRepository _repository;
    private readonly Mock<ILogger<FilterService>> _loggerMock;
    private readonly FilterService _sut;

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

    [Fact]
    public async Task ApplyFilterAsync_FilterNotFound_ThrowsInvalidFilterException()
    {
        // Arrange
        var image = CreateValidImage();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _sut.ApplyFilterAsync(image, nonExistentId).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidFilterException>().ConfigureAwait(false);
    }

    [Fact]
    public async Task ApplyFilterAsync_InactiveFilter_ThrowsInvalidFilterException()
    {
        // Arrange
        var image = CreateValidImage();
        var config = CreateFilterConfig("Inactive Blur", FilterType.Blur, isActive: false);
        var saved = await _repository.CreateAsync(config).ConfigureAwait(false);

        // Act
        var act = async () => await _sut.ApplyFilterAsync(image, saved.Id).ConfigureAwait(false);

        // Assert
        var assertion = await act.Should().ThrowAsync<InvalidFilterException>().ConfigureAwait(false);
        assertion.Which.Message.Should().Contain("not active");
    }

    [Fact]
    public async Task ApplyFilterAsync_GrayscaleFilter_SetsColorSpaceToGrayscale()
    {
        // Arrange
        var image = CreateValidImage(ColorSpace.Rgb);
        var config = CreateFilterConfig("Grayscale", FilterType.Grayscale);
        var saved = await _repository.CreateAsync(config).ConfigureAwait(false);

        // Act
        var result = await _sut.ApplyFilterAsync(image, saved.Id).ConfigureAwait(false);

        // Assert
        result.ColorSpace.Should().Be(ColorSpace.Grayscale);
        result.BitsPerPixel.Should().Be(8);
    }

    [Fact]
    public async Task CreateFilterAsync_NullConfig_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _sut.CreateFilterAsync(null!).ConfigureAwait(false);

        // Assert
        var assertion = await act.Should().ThrowAsync<ArgumentNullException>().ConfigureAwait(false);
        assertion.Which.ParamName.Should().Be("config");
    }

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
        var act = async () => await _sut.CreateFilterAsync(config).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidFilterException>().ConfigureAwait(false);
    }

    [Fact]
    public async Task GetFiltersByTypeAsync_MultipleTypesStored_ReturnsOnlyMatchingType()
    {
        // Arrange
        await _repository.CreateAsync(CreateFilterConfig("Blur A", FilterType.Blur)).ConfigureAwait(false);
        await _repository.CreateAsync(CreateFilterConfig("Sharpen A", FilterType.Sharpen)).ConfigureAwait(false);
        await _repository.CreateAsync(CreateFilterConfig("Grayscale A", FilterType.Grayscale)).ConfigureAwait(false);

        // Act
        var results = await _sut.GetFiltersByTypeAsync(FilterType.Blur).ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(1);
        results.Should().OnlyContain(f => f.FilterType == FilterType.Blur);
    }
}
