# Contributing to GPU Image Processing

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Assume good intentions
- Report issues through proper channels

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Code editor (VS Code, Visual Studio, or JetBrains Rider)
- GPU with OpenCL 1.2+ (optional, CPU fallback available)

### Setup Development Environment

```bash
# Clone repository
git clone https://github.com/Sarmkadan/gpu-image-processing.git
cd gpu-image-processing

# Setup development environment
./build.sh check
./build.sh restore

# Build project
./build.sh build

# Run tests
./build.sh test
```

### Using Make

```bash
# List all available commands
make help

# Build and test
make install
make test

# Format code
make format

# Run analysis
make lint
```

## Development Workflow

### 1. Create a Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/bug-description
```

Branch naming conventions:
- `feature/*` - New features
- `fix/*` - Bug fixes
- `docs/*` - Documentation
- `refactor/*` - Code refactoring
- `perf/*` - Performance improvements

### 2. Make Changes

Follow code style guidelines:
- Use the provided `.editorconfig` file
- Format code: `dotnet format`
- Analyze code: `dotnet build /p:EnforceCodeStyleInBuild=true`

File structure:
- Each .cs file should start with the copyright header
- Use meaningful class and method names
- Add XML documentation to public members
- Keep methods focused and testable

### 3. Write Tests

For new features:
```csharp
[Fact]
public async Task ProcessImage_WithGaussianFilter_ReturnsBlurredResult()
{
    // Arrange
    var imageService = new ImageProcessingService(...);
    var image = await imageService.RegisterImageAsync("test.jpg", "Test");

    // Act
    var result = await imageService.ProcessImageAsync(...);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Success", result.Status);
}
```

Run tests:
```bash
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### 4. Commit Changes

Commit messages should be clear and descriptive:

```bash
# Good commit messages
git commit -m "Add Canny edge detection filter

- Implement Canny algorithm with threshold parameters
- Add unit tests for edge detection
- Update documentation with usage examples"

# Avoid
git commit -m "Fix bug"
git commit -m "Changes"
```

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Visit GitHub and create a pull request with:
- Clear title describing the change
- Description of what and why
- Related issues (if any)
- Screenshots or examples (if applicable)

## Code Style Guide

### C# Conventions

```csharp
// Class naming: PascalCase
public class ImageProcessingService
{
    // Public properties: PascalCase
    public string Name { get; set; }

    // Private fields: camelCase with underscore prefix
    private int _processingCount;

    // Method naming: PascalCase
    public async Task<ProcessingResult> ProcessImageAsync(Guid imageId)
    {
        // Local variables: camelCase
        var startTime = DateTime.UtcNow;

        // Use var for obvious types
        var result = new ProcessingResult();

        // Explicit for unclear types
        List<ProcessingJob> jobs = await GetJobsAsync();

        return result;
    }

    // Interface implementations
    public interface IService { }

    // Constants: UPPER_CASE
    private const int MAX_RETRIES = 3;
}
```

### Formatting

- Indentation: 4 spaces
- Line length: Max 120 characters
- Braces: Allman style (opening brace on new line)
- Use `var` for obvious types, explicit for clarity

Run formatter:
```bash
dotnet format
```

### File Headers

Each C# file must start with a copyright header. **Do not remove or modify existing author headers** when editing files.

```csharp
// Copyright (c) [year] [Author Name]. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GpuImageProcessing.Services;
```

### Comments

Write comments for "why", not "what":

```csharp
// Good - explains the reason
// Process in batches to avoid GPU memory exhaustion
var batches = SplitIntoBatches(items, batchSize);

// Avoid - restates the code
// Loop through items
foreach (var item in items) { }

// Use XML docs for public APIs
/// <summary>
/// Processes an image with specified filters.
/// </summary>
/// <param name="imageId">The image identifier</param>
/// <param name="filterIds">List of filter identifiers</param>
/// <returns>Processing result with status and output path</returns>
/// <exception cref="ImageProcessingException">Thrown when processing fails</exception>
public async Task<ProcessingResult> ProcessImageAsync(
    Guid imageId,
    List<Guid> filterIds)
```

## Testing Guidelines

### Unit Tests

```csharp
[Fact]
public async Task CreateFilter_WithValidType_ReturnsFilter()
{
    // Arrange - Set up test data
    var service = new FilterService();
    var filterType = FilterType.Gaussian;

    // Act - Execute the action
    var result = await service.CreateFilterAsync(
        filterType,
        "TestFilter",
        "Test description"
    );

    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal(filterType, result.Type);
}
```

### Test Naming

- Describe what is being tested
- Describe the scenario
- Describe the expected result

```
MethodName_Scenario_ExpectedResult
CreateFilter_WithValidType_ReturnsFilter
ProcessImage_WithMultipleFilters_AppliesAllFilters
GetMetrics_WhileProcessing_ReturnsAccurateUtilization
```

### Coverage

Aim for >80% code coverage:
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Performance Considerations

- GPU memory is limited - use batching
- Avoid synchronous I/O - use async/await
- Cache expensive computations
- Profile before optimizing: `make bench`
- Consider CPU fallback for edge cases

Example:
```csharp
// Good - asynchronous, uses caching
public async Task<ProcessingResult> ProcessImageAsync(Guid imageId)
{
    var cached = await cache.GetAsync(imageId);
    if (cached != null) return cached;

    var result = await ProcessOnGpuAsync(imageId);
    await cache.SetAsync(imageId, result, TimeSpan.FromHours(1));
    return result;
}
```

## Documentation

### README and Docs

- Keep examples up-to-date
- Document new features
- Update API reference
- Include troubleshooting steps

### Code Comments

- Explain non-obvious logic
- Link to related code
- Note performance considerations
- Document workarounds

## Pull Request Checklist

Before submitting:

- [ ] Code compiles without errors
- [ ] All tests pass: `dotnet test`
- [ ] Code formatted: `dotnet format`
- [ ] Analysis passes: `dotnet build /p:EnforceCodeStyleInBuild=true`
- [ ] XML docs added to public members
- [ ] Tests written for new features
- [ ] Coverage maintained (>80%)
- [ ] Commit messages are clear
- [ ] No breaking changes (or documented)
- [ ] Documentation updated if needed

## Review Process

1. **Code Review**: Maintainers review for quality, style, and functionality
2. **Tests**: CI/CD runs automated checks
3. **Feedback**: Address requested changes
4. **Approval**: Maintainers approve and merge

Expected review time: 1-3 days

## Reporting Issues

### Bug Report

```markdown
## Description
Brief description of the bug

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: Windows 11
- .NET Version: 10.0
- GPU: NVIDIA RTX 3080
- Driver Version: 535.104

## Logs
```
Paste relevant error logs
```
```

### Feature Request

```markdown
## Description
What feature would you like?

## Use Case
Why do you need this?

## Proposed Solution
How should it work?

## Alternatives
Other ways to solve this?
```

## Release Process

Maintainers follow semantic versioning:
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

Releasing:
```bash
git tag v1.2.0
git push --tags
```

GitHub Actions automatically:
1. Builds release
2. Runs full test suite
3. Creates GitHub Release
4. Publishes to NuGet (if configured)

## Community

- **Discussions**: Use GitHub Discussions for questions
- **Issues**: Report bugs and feature requests
- **Changelog**: See [CHANGELOG.md](CHANGELOG.md)
- **Roadmap**: Check GitHub Issues with "roadmap" label

## License

By contributing, you agree to license your contributions under the MIT License (see LICENSE file).

## Recognition

Contributors will be recognized in:
- CHANGELOG.md
- GitHub contributors list
- Project documentation

Thank you for contributing! 🚀

---

**Questions?** Feel free to:
- Ask in GitHub Discussions
- Open an issue
- Contact via GitHub profile

For more information, see:
- [README.md](README.md) - Project overview
- [ARCHITECTURE.md](ARCHITECTURE.md) - Architecture documentation
- [docs/api-reference.md](docs/api-reference.md) - API documentation
