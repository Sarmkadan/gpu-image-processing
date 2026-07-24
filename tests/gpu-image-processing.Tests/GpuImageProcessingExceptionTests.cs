using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Exceptions;
using Xunit;

namespace GpuImageProcessing.Tests;

public class GpuImageProcessingExceptionTests
{
    private class TestException : GpuImageProcessingException
    {
        public TestException(string message, int? errorCode = null)
            : base(message, errorCode) { }

        public TestException(string message, Exception innerException, int? errorCode = null)
            : base(message, innerException, errorCode) { }
    }

    [Fact]
    public void Constructor_WithErrorCode_SetsProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var ex = new TestException("Test message", 42);

        // Assert
        ex.ErrorCode.Should().Be(42);
        ex.OccurredAt.Should().BeOnOrAfter(now.AddSeconds(-5));
        ex.OccurredAt.Should().BeOnOrBefore(now.AddSeconds(5));
    }

    [Fact]
    public void Constructor_WithoutErrorCode_SetsErrorCodeToNull()
    {
        var ex = new TestException("Test message");
        ex.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void ToString_IncludesMessageErrorCodeAndOccurred()
    {
        var ex = new TestException("Test message", 99);
        var str = ex.ToString();

        str.Should().Contain("Test message");
        str.Should().Contain("Error Code: 99");
        str.Should().Contain("Occurred:");
    }

    [Fact]
    public void ToString_WithoutErrorCode_DoesNotContainErrorCode()
    {
        var ex = new TestException("Test message");
        var str = ex.ToString();

        str.Should().NotContain("Error Code:");
    }

    [Fact]
    public void ToString_WithInnerException_IncludesInnerMessage()
    {
        var inner = new InvalidOperationException("Inner exception");
        var ex = new TestException("Test message", inner);
        var str = ex.ToString();

        str.Should().Contain("Inner exception");
    }

    [Fact]
    public void Constructor_WithNullMessage_DoesNotThrow()
    {
        var ex = new TestException(null);
        var str = ex.ToString();

        str.Should().Contain("System.Exception");
    }

    [Fact]
    public void Constructor_WithEmptyMessage_DoesNotThrow()
    {
        var ex = new TestException(string.Empty);
        var str = ex.ToString();

        str.Should().Contain("System.Exception");
    }

    [Fact]
    public void Constructor_WithBoundaryErrorCodes_SetsCorrectValues()
    {
        var exMin = new TestException("Min", int.MinValue);
        var exMax = new TestException("Max", int.MaxValue);

        exMin.ErrorCode.Should().Be(int.MinValue);
        exMax.ErrorCode.Should().Be(int.MaxValue);
    }

    [Fact]
    public void AllPublicExceptionTypes_DeriveFrom_GpuImageProcessingException()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(GpuImageProcessingException))!;
        var exceptionTypes = assembly.GetTypes()
            .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract)
            .Where(t => typeof(Exception).IsAssignableFrom(t))
            .Where(t => t != typeof(Exception))
            .ToList();

        // Act
        var exceptionsNotDerivingFromBase = exceptionTypes
            .Where(t => !typeof(GpuImageProcessingException).IsAssignableFrom(t))
            .ToList();

        // Assert
        exceptionsNotDerivingFromBase.Should().BeEmpty(
            $"All public exception types should derive from {nameof(GpuImageProcessingException)}. " +
            $"Found: {string.Join(", ", exceptionsNotDerivingFromBase.Select(t => t.Name))}");
    }
}
