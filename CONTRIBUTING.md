# Contributing

Thanks for helping out. This repo is a C# image processing library (NuGet package `Zaiets.gpu.image.processing`) with a console demo app, tests and benchmarks. By taking part you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisites

- .NET 10 SDK. `global.json` pins `10.0.100` with `rollForward: latestMinor`, so any 10.0.x SDK works.
- Git.
- A GPU is optional. Tests run on the CPU fallback path and don't need OpenCL.

## Build

```bash
dotnet restore
dotnet build gpu-image-processing.sln
```

The solution contains:

| Project | What it is |
|---|---|
| `GpuImageProcessing.csproj` | Console app. Compiles the root folders plus `src/**`. |
| `gpu-image-processing.csproj` | The library that gets packaged. Compiles `src/**` only. |
| `tests/gpu-image-processing.Tests` | xUnit tests. |
| `benchmarks/gpu-image-processing.Benchmarks` | BenchmarkDotNet suite. |

`src/` is compiled into both the library and the console app on purpose. Don't "fix" that by adding a project reference.

## Test

```bash
dotnet test gpu-image-processing.sln
dotnet test --filter "FullyQualifiedName~FilterChainBuilderTests"   # one class
```

Tests use xUnit, FluentAssertions and Moq. They must pass without a GPU.

## Coding conventions

- C# with `LangVersion=latest`, `Nullable` enabled, `ImplicitUsings` enabled, file-scoped namespaces.
- Follow `.editorconfig`: 4-space indent, UTF-8, final newline, no trailing whitespace. Run `dotnet format` before committing.
- Keep the build free of new warnings. Warnings do not fail the build (`TreatWarningsAsErrors` is `false` in `Directory.Build.props`), but reviewers will ask you to fix any you introduce.
- Add XML doc comments to public members.
- Use `ArgumentNullException.ThrowIfNull` and `ArgumentException` with a parameter name for argument guards.
- Put limits and magic numbers in `AppConstants` (`src/Core/Constants.cs`).
- Keep existing author headers untouched. New `.cs` files copy the header of the files next to them.
- Name tests `Method_Scenario_ExpectedResult`, for example `Create_BlankName_ThrowsArgumentException`.

## Branches

Fork the repo and branch from `main`. Use a prefix that matches the change:

- `feature/<short-name>`
- `fix/<short-name>`
- `refactor/<short-name>`
- `perf/<short-name>`
- `docs/<short-name>`

## Commit messages

Use the conventional prefix style the history follows, with a lowercase imperative subject under about 72 characters:

```
fix: reject blank filter names in FilterChainBuilder

Throw ArgumentException with parameter name "name" instead of
silently accepting whitespace.
```

Common prefixes: `feat`, `fix`, `refactor`, `perf`, `docs`, `test`, `ci`, `chore`. Explain the why in the body when it isn't obvious from the diff.

## Pull requests

1. Push your branch to your fork and open a PR against `main`.
2. Keep it focused: one behavior change per PR. Put refactors in a separate PR from behavior changes.
3. Add or update tests for any behavior change.
4. Describe what changed and why, and note how you tested it.
5. CI must pass. The `CI` workflow restores, builds in Release, and runs the tests in Release. CodeQL also runs.
6. Request a review. Address feedback with new commits or an amended commit, whichever the reviewer prefers.

For security issues, don't open a public issue. Follow [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
