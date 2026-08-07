namespace GpuImageProcessing.Tests;

using System.Collections.Generic;
using System.Linq;
using Xunit;
using GpuImageProcessing.Fallback;
using Microsoft.Extensions.Logging.Abstractions;

public class CpuImageProcessorValidationTests
{
    private readonly CpuImageProcessor _validProcessor;

    public CpuImageProcessorValidationTests()
    {
        // Create a valid CpuImageProcessor instance with a null logger (using NullLogger)
        _validProcessor = new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance);
    }

    [Fact]
    public void Validate_ReturnsEmptyListForValidProcessor()
    {
        // Act
        var result = _validProcessor.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullExceptionForNullProcessor()
    {
        // Act & Assert
        CpuImageProcessor? nullProcessor = null;
        Assert.Throws<ArgumentNullException>(() => nullProcessor.Validate());
    }

    [Fact]
    public void IsValid_ReturnsTrueForValidProcessor()
    {
        // Act
        var result = _validProcessor.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullExceptionForNullProcessor()
    {
        // Act & Assert
        CpuImageProcessor? nullProcessor = null;
        Assert.Throws<ArgumentNullException>(() => nullProcessor.IsValid());
    }

    [Fact]
    public void EnsureValid_DoesNotThrowForValidProcessor()
    {
        // Act & Assert
        Assert.NotNull(Record.Exception(() => _validProcessor.EnsureValid()));
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullExceptionForNullProcessor()
    {
        // Act & Assert
        CpuImageProcessor? nullProcessor = null;
        Assert.Throws<ArgumentNullException>(() => nullProcessor.EnsureValid());
    }
}