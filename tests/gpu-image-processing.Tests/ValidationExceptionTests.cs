using System;
using System.Collections.Generic;
using GpuImageProcessing.Exceptions;
using Xunit;

namespace GpuImageProcessing.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        // Arrange
        var message = "Validation failed";
        var entityName = "TestEntity";
        var errors = new Dictionary<string, string>
        {
            { "Field1", "Error1" },
            { "Field2", "Error2" }
        };
        var errorCode = 1001;

        // Act
        var ex = new ValidationException(message, entityName, errors, errorCode);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Equal(entityName, ex.EntityName);
        Assert.Same(errors, ex.ValidationErrors);
        Assert.Equal(errorCode, ex.ErrorCode);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsProperties()
    {
        // Arrange
        var message = "Validation failed with inner";
        var inner = new InvalidOperationException("inner");
        var entityName = "InnerEntity";
        var errorCode = 2002;

        // Act
        var ex = new ValidationException(message, inner, entityName, errorCode);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal(entityName, ex.EntityName);
        Assert.Null(ex.ValidationErrors);
        Assert.Equal(errorCode, ex.ErrorCode);
    }

    [Fact]
    public void ToString_IncludesEntityAndValidationErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string>
        {
            { "Name", "Required" },
            { "Age", "Must be positive" }
        };
        var ex = new ValidationException("Invalid data", "Person", errors);

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("Invalid data", result);
        Assert.Contains("Entity: Person", result);
        Assert.Contains("Validation Errors:", result);
        Assert.Contains("- Name: Required", result);
        Assert.Contains("- Age: Must be positive", result);
    }

    [Fact]
    public void ToString_WithoutEntityOrErrors_OmitsOptionalSections()
    {
        // Arrange
        var ex = new ValidationException("Just a message");

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("Just a message", result);
        Assert.DoesNotContain("Entity:", result);
        Assert.DoesNotContain("Validation Errors:", result);
    }

    [Fact]
    public void ToString_WithEmptyValidationErrors_OmitsErrorsSection()
    {
        // Arrange
        var emptyErrors = new Dictionary<string, string>();
        var ex = new ValidationException("Empty errors", "EmptyEntity", emptyErrors);

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("Entity: EmptyEntity", result);
        Assert.DoesNotContain("Validation Errors:", result);
    }

    [Fact]
    public void ToString_WithNullValidationErrors_OmitsErrorsSection()
    {
        // Arrange
        var ex = new ValidationException("Null errors", "NullEntity", null);

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("Entity: NullEntity", result);
        Assert.DoesNotContain("Validation Errors:", result);
    }
}
