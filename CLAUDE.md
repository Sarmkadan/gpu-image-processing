# CLAUDE.md

## Project overview

GPU-accelerated image processing library and demo console app in C# (.NET 10) using OpenCL via Silk.NET, with a CPU/SIMD fallback; shipped as NuGet package `Zaiets.gpu.image.processing`.

## Build

SDK: .NET 10 (`global.json` pins `10.0.100`, rollForward latestMinor).

```bash
dotnet restore
dotnet build                          # Debug, whole solution (gpu-image-processing.sln)
dotnet build -c Release
dotnet build gpu-image-processing.csproj   # library only (src/**)
dotnet build GpuImageProcessing.csproj     # console exe (root folders + src/**)
dotnet run --project GpuImageProcessing.csproj
make build | make release | make publish | make package   # Makefile wrappers
./build.sh                            # dotnet build + dotnet test --no-build
```

Solution has four projects sharing one directory tree:

| Project | Output | Compiles |
|---|---|---|
| `GpuImageProcessing.csproj` | console exe | root folders (`Api/`, `Cli/`, `Core/`, `Events/`, `Middleware/`, `Services/`, ...) plus `src/**` |
| `gpu-image-processing.csproj` | class library / NuGet | only `src/**` (`EnableDefaultCompileItems=false`) |
| `tests/gpu-image-processing.Tests` | xUnit | references the exe project |
| `benchmarks/gpu-image-processing.Benchmarks` | BenchmarkDotNet | references the exe project |

The two main projects use separate `obj\<name>\` and `bin\<name>\` base paths to avoid MSBuild collisions. `src/` is compiled twice (once per project) - intentional, do not "fix" by adding a project reference.

## Test

xUnit + FluentAssertions 7 + Moq. Tests live in `tests/gpu-image-processing.Tests/`.

```bash
dotnet test                                   # all tests
dotnet test --no-build -c Release             # what CI runs
dotnet test --filter "FullyQualifiedName~FilterChainBuilderTests"
make test | make test-coverage | make test-watch
```

Conventions:
- Test class `<TypeName>Tests`, method `Method_Scenario_ExpectedResult` (e.g. `Create_BlankName_ThrowsArgumentException`).
- Assertions via FluentAssertions (`act.Should().Throw<ArgumentException>().WithParameterName("name")`).
- Layout mirrors source: `Domain/`, `Pipeline/`, `Services/`, `Fallback/`, `Imaging/`, plus `Integration/` (end-to-end, concurrency), `Golden/` (golden-image regression, CPU fallback path), `fixtures/` (copied to output, `LinkBase="fixtures"`).
- Tests must run without a GPU; the CPU fallback is the test path.

## Lint / Format

`.editorconfig` at root (4-space indent, PascalCase types/members, `I`-prefixed interfaces; severity is `suggestion`). `TreatWarningsAsErrors=false`.

```bash
dotnet format                                 # make format
dotnet build /p:EnforceCodeStyleInBuild=true  # make lint
```

CI (`.github/workflows/ci.yml`): restore, `dotnet build -c Release`, `dotnet test -c Release` with trx logger. Also CodeQL, Docker, NuGet publish, release workflows.

## Architecture

Detailed doc: `docs/ARCHITECTURE.md` (root `ARCHITECTURE.md` just points there). Per-type notes in `docs/*.md`.

Two generations of code coexist:

1. `src/` - the library core, namespaces `GpuImageProcessing.*` (no `.Core.` segment). This is what gets packaged.
   - `src/Domain/` - `Image`, `ImageBatch`, `ProcessingResult`, `FilterConfiguration`/`FilterChain`/`FilterChainBuilder`, `GpuDevice`, `WorkgroupConfiguration`, `ComputeShaderPass`, `SimdCapabilities`.
   - `src/Core/` - `Constants.cs` (`AppConstants`), `Enums.cs` (`FilterType` etc.), `GpuException`, `ProcessingException`.
   - `src/Exceptions/` - `GpuImageProcessingException` (abstract base), `ConfigurationException`, `ValidationException`.
   - `src/Pipeline/` - `IComputeShaderPipeline`, `ComputeShaderPipeline`, `ResilientComputeShaderPipeline`, `BatchProcessingPipeline`, `WorkgroupOptimizer`.
   - `src/Fallback/` - `IImageProcessor` (backend seam), `CpuImageProcessor`.
   - `src/Imaging/` - `IImageCodec`, `ImageCodecRegistry`, PPM/PGM (`PortablePixmap`).
   - `src/Services/` - `GpuManagementService`, `ImageProcessingService`, `FilterService`, `BatchProcessingService`, `PerformanceMonitoringService`, `SimdFallbackService`.
   - `src/Repository/` - in-memory repositories behind `IRepository`.
   - `src/Configuration/` - `AppSettings`, `DependencyInjectionExtensions.AddGpuImageProcessing(...)`.
   - `src/Batch/` - `DirectoryBatchProcessor`.
2. Root folders - the application shell, namespaces `GpuImageProcessing.Core.*`. Entry point `Program.cs` (`DependencyInjectionSetup.CreateAndInitializeServiceProviderAsync`). `Cli/` (`CliParser`, `CommandDispatcher`, `*Command`, `InteractiveShell`), `Api/` (`ImageProcessingController`, `ApiResponse`, `RequestValidator`), `Middleware/`, `Events/` (event aggregator), `BackgroundWorkers/`, `Formatters/`, `Caching/`, `Monitoring/` (`HealthCheckService`), `Utilities/`, `Services/` (telemetry, notifications, async task queue).

Note: `Core/` at root and `src/Core/` are different things (app-shell `GpuImageProcessing.Core.*` vs library constants/enums).

## Conventions

- File header on every `.cs`: `#nullable enable` + the `Author: Vladyslav Zaiets | https://sarmkadan.com` banner. File-scoped namespaces, `Nullable` and `ImplicitUsings` enabled, `AllowUnsafeBlocks=true`, `LangVersion=latest`. XML doc comments on all public members (`GenerateDocumentationFile=true`).
- Sibling-file split per type: `Foo.cs` (data/behavior), `FooExtensions.cs` (helpers), `FooValidation.cs` (`Validate(this Foo)` returning `IReadOnlyList<string>` of problems), `FooJsonExtensions.cs` (System.Text.Json helpers). Follow this when adding a type; do not merge them.
- Argument guards: `ArgumentNullException.ThrowIfNull(x)`, `ArgumentException` with parameter name for bad strings. Domain errors derive from `GpuImageProcessingException` (carries `ErrorCode`, `OccurredAt`); see `EXCEPTION_UNIFICATION_SUMMARY.md`.
- DI: Microsoft.Extensions.DependencyInjection, everything registered as singleton in `AddGpuImageProcessing`. Consumers depend on `IImageProcessor`, not `CpuImageProcessor`; a GPU backend replaces that single registration.
- Limits/magic numbers go in `AppConstants` (`src/Core/Constants.cs`).
- Build is dirty with generated noise (`build_output.txt`, `clean_build.txt`, `README.md.backup`, `.aider.*`, stray `}` file); ignore, do not commit new ones. Do not keep changelogs.
