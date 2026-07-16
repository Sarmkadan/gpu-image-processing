# Architecture

This document describes the solution as it exists in the repository today - what compiles
where, how data flows through the processing pipeline, and why the bigger design decisions
were made. If a component is not mentioned in the code, it is not mentioned here.

## Solution layout

The solution (`gpu-image-processing.sln`) contains four projects:

| Project | File | Output | What it compiles |
|---|---|---|---|
| `GpuImageProcessing` | `GpuImageProcessing.csproj` | console exe | everything in the repo root folders (`Api/`, `Cli/`, `Core/`, `Events/`, `Middleware/`, ...) **plus** `src/**` (default compile items, with `tests/`, `benchmarks/`, `examples/`, `docs/` removed) |
| `gpu-image-processing` | `gpu-image-processing.csproj` | class library / NuGet package | **only** `src/**` (`EnableDefaultCompileItems=false`, explicit `<Compile Include="src\**\*.cs" />`) |
| `gpu-image-processing.Tests` | `tests/...` | xUnit test suite | references the exe project |
| `gpu-image-processing.Benchmarks` | `benchmarks/...` | BenchmarkDotNet suite | references the exe project |

The two "main" projects deliberately share `src/` but keep separate `obj/`/`bin/` base paths
(set at the top of each csproj) so they can live in one directory without MSBuild artifact
collisions.

### Why two projects over the same tree

The codebase has two generations of code that coexist:

1. **`Core/` + root folders** (`GpuImageProcessing.Core.*` namespaces) - the original
   application: demo entry point (`Program.cs`), in-memory repositories, service layer
   (`ImageProcessingService`, `FilterService`, `TransformService`, `BatchProcessingService`,
   `DeviceService`), plus application-shell concerns: CLI commands, an API facade,
   middleware chain, event aggregator, background workers, result formatters, caching,
   monitoring.
2. **`src/`** (`GpuImageProcessing.*` namespaces, no `.Core.` segment) - the newer library
   core that is shipped as the NuGet package (`Zaiets.gpu.image.processing`): domain model,
   OpenCL device management via Silk.NET, compute-shader pipeline, CPU/SIMD fallback,
   batch pipeline, PPM/PGM imaging.

The library project exists so the reusable core can be packaged without dragging the demo
app, CLI and middleware along. The exe still compiles `src/` directly (not via project
reference) - a historical artifact; the trade-off accepted here is duplicate compilation of
`src/` in exchange for not restructuring the exe's root-level folders.

## The library core (`src/`)

### Domain (`src/Domain/`)

Plain, mostly mutable POCOs plus a few immutable value types:

- `Image`, `ImageBatch`, `ProcessingResult`, `PerformanceMetrics` - data carriers.
- `FilterConfiguration` / `FilterChain` / `FilterChainBuilder` - a filter is a validated
  parameter bag keyed by `FilterType` (enum in `src/Core/Enums.cs`); chains compose filters
  and the builder gives a fluent API over chain construction.
- `GpuDevice`, `WorkgroupConfiguration`, `ComputeShaderPass` - GPU-side descriptors:
  device capabilities, workgroup layout, and one shader dispatch respectively.
- `SimdCapabilities` - runtime CPU SIMD probe (SSE2 ... AVX-512F) used to pick vectorised
  code paths in the fallback layer.

Almost every domain type comes with three sibling files: `*Extensions.cs` (behavioral
helpers), `*Validation.cs` (guard/validation helpers) and `*JsonExtensions.cs`
(System.Text.Json serialization helpers). This is a deliberate partial-by-convention split:
the entity file stays a readable data definition while serialization and validation code -
which is bulky and mechanical - lives next door. The cost is a large file count; the
benefit is that no file mixes concerns.

### Services (`src/Services/`)

- `GpuManagementService` - the only place that talks to the OpenCL runtime
  (`Silk.NET.OpenCL`). At startup it enumerates platforms/devices and maps them into
  `GpuDevice` domain objects. **Design decision:** if no OpenCL platform or device is found
  (or native interop fails), it logs a warning, sets `IsCpuFallbackActive`, and registers a
  simulated device (`AddSimulatedDevice`). Rationale: the rest of the system - services,
  pipeline, tests, CI containers without GPUs - can run unchanged against a stable device
  abstraction. Trade-off: a "device" in the repository is not guaranteed to be real
  hardware; callers must check the fallback flag if they care.
- `ImageProcessingService` - orchestrates a single-image run: loads the image from
  `ImageRepository`, resolves filter configs, delegates the actual pixel work, records a
  `ProcessingResult`.
- `FilterService`, `BatchProcessingService`, `PerformanceMonitoringService` - filter CRUD +
  validation, multi-image job orchestration, and metrics collection respectively.
- `SimdFallbackService` - picks the best vectorised CPU path based on
  `SimdCapabilities.Detect()`.

### Pipeline (`src/Pipeline/`)

- `IComputeShaderPipeline` / `ComputeShaderPipeline` - executes an ordered list of
  `ComputeShaderPass`es against a device: passes are sorted by priority, passes without an
  explicit `WorkgroupConfiguration` get one from `IWorkgroupOptimizer`, per-pass telemetry
  is accumulated and exposed via `GetStatisticsAsync`.
- `IWorkgroupOptimizer` / `WorkgroupOptimizer` - computes workgroup layouts from device
  limits. Kept behind an interface because layout heuristics are the most likely thing to
  be swapped (per-vendor tuning) and the easiest thing to stub in tests.
- `BatchProcessingPipeline` - fan-out execution of a batch with a `SemaphoreSlim` bounding
  concurrency at `BatchPipelineOptions.MaxConcurrency`, progress surfaced through
  `BatchPipelineProgressEventArgs`. **Decision:** semaphore + tasks instead of
  `Parallel.ForEachAsync`/Dataflow - the simplest primitive that gives bounded concurrency,
  cancellation, and per-item error isolation (`BatchItemResult` per file, never one failure
  aborting the batch).

### Fallback (`src/Fallback/`)

- `IImageProcessor` - the backend contract: `CanProcess(FilterType)`,
  `ApplyFilterAsync(...)`, `Resize(...)`, grayscale conversion. This is the seam between
  "what to do" (services, pipeline) and "how pixels get touched" (a backend).
- `CpuImageProcessor` - the pure-CPU implementation (`CpuFilters` holds the kernels).
  It is the reference implementation: byte-exact, dependency-free, always available.
  A GPU-backed `IImageProcessor` plugs in at the same seam - that is the primary
  extension point of the library.

### Imaging (`src/Imaging/`)

- `PortablePixmap` - minimal binary Netpbm reader/writer (P6/PPM, P5/PGM). **Decision:**
  Netpbm instead of an imaging dependency for the core: the formats are byte-exact and
  trivial to round-trip, which makes golden-image regression tests deterministic and keeps
  the packaged library free of native/managed image codec dependencies. Trade-off: JPEG/PNG
  handling is out of scope for the core and must be layered on top by the consumer.

### Batch (`src/Batch/`)

- `DirectoryBatchProcessor` - walks a directory for `.ppm`/`.pgm` files and runs an
  `IImageProcessor` over them, reporting `BatchProgress` through `IProgress<T>` and
  returning a `BatchRunSummary` of per-file `BatchItemResult`s. Deliberately UI-free so the
  CLI and tests share the exact same code path.

### Repositories (`src/Repository/`)

`IRepository<T>` with in-memory, thread-safe implementations (`ImageRepository`,
`FilterConfigurationRepository`, `ProcessingResultRepository`). **Decision:** in-memory on
purpose - the library processes transient pixel data; durable storage is a consumer
concern. The interface exists so a consumer can substitute a persistent implementation
without touching the services.

### Configuration (`src/Configuration/`)

- `AppSettings` - options object bound from the `AppSettings` configuration section.
- `DependencyInjectionExtensions.AddGpuImageProcessing(IServiceCollection, IConfiguration)` -
  single composition-root helper that registers repositories, services, the batch pipeline
  and bound settings; `AddGpuImageProcessingLogging` sets the console logger with sane
  category filters. Everything is registered as singletons: the services are stateless or
  internally synchronized, and the repositories are process-wide in-memory stores.

## The application shell (root folders, exe only)

These compile only into the console executable and consume the `Core/` generation of
services:

- `Program.cs` - demo entry point: builds the container via
  `Core/Configuration/DependencyInjectionSetup`, enumerates devices, creates sample
  filters/transforms/jobs and prints statistics. It is a showcase, not a daemon.
- `Cli/` - hand-rolled command layer (`CliParser`, `CommandDispatcher`, commands like
  `ProcessImageCommand`, `BatchDirectoryCommand`, `InteractiveShell`).
- `Api/` - `ImageProcessingController` + `ApiResponse`/`RequestValidator`. Note: the
  project references no ASP.NET Core packages - this is a transport-agnostic facade shaped
  like a controller, not a hosted web API.
- `Middleware/` - `IProcessingMiddleware` chain (`ProcessingPipeline`) with logging,
  error-handling, rate-limiting, compression and authorization steps around processing
  requests (again: processing middleware, not HTTP middleware).
- `Events/` - `EventAggregator`/`EventPublisher` pub-sub with domain events
  (`ImageRegisteredEvent`, processing events) to decouple workers/monitoring from services.
- `BackgroundWorkers/` - `JobProcessingWorker`, `HealthCheckWorker`,
  `CacheMaintenanceWorker`, `MetricsAggregationWorker`.
- `Formatters/` - `IResultFormatter` with Text/Json/Csv/Html/Markdown/Xml implementations
  for rendering processing results.
- `Caching/`, `Monitoring/`, `Integration/`, `Utilities/` - processing cache + distributed
  cache abstraction, health checks, HTTP/webhook/database integration stubs, and shared
  helpers.

## Data flow

Single image (library core):

```
consumer
  -> AddGpuImageProcessing(...)            composition root
  -> ImageProcessingService.ProcessImageAsync(imageId, filterIds)
       -> ImageRepository                  load domain Image
       -> FilterConfigurationRepository    resolve FilterConfigurations
       -> IImageProcessor backend          CpuImageProcessor today; GPU backend via the same seam
       -> ProcessingResultRepository       persist ProcessingResult (in-memory)
```

Directory batch (CLI / tests):

```
BatchDirectoryCommand
  -> DirectoryBatchProcessor.ProcessDirectoryAsync
       -> PortablePixmap.Read              P5/P6 -> row-major byte buffer
       -> IImageProcessor                  filter/resize/grayscale
       -> PortablePixmap.Write             byte-exact round-trip
       -> IProgress<BatchProgress>         UI-agnostic progress
  <- BatchRunSummary (per-file success/error, nothing aborts the batch)
```

GPU dispatch:

```
ComputeShaderPipeline.ExecuteAsync(passes, deviceId)
  -> sort passes by Priority
  -> WorkgroupOptimizer                    fill in missing WorkgroupConfigurations
  -> GpuManagementService                  device selection (highest score on Guid.Empty)
  -> per-pass execution + telemetry -> PipelineExecutionResult
```

## Extension points

- **`IImageProcessor`** - add a processing backend (real OpenCL kernels, SIMD-specialised
  CPU paths). `DirectoryBatchProcessor` and the services depend on the interface only.
- **`IRepository<T>`** - substitute persistent storage.
- **`IWorkgroupOptimizer`** - vendor-specific workgroup tuning.
- **`IComputeShaderPipeline`** - alternative dispatch strategies.
- **`IResultFormatter` / `IProcessingMiddleware`** (exe shell) - new output formats and
  cross-cutting steps.

## Known limitations

- `GpuManagementService` enumerates OpenCL devices, but the shipped `IImageProcessor`
  implementation is CPU-only; kernel compilation/dispatch for real hardware is not wired
  end to end. Without OpenCL present the device list contains a clearly-labelled simulated
  device.
- Repositories are in-memory; nothing survives a restart.
- The core reads/writes only PPM/PGM. Other formats are the consumer's job.
- The exe compiles `src/` directly instead of referencing the library project, so the two
  projects can drift in build settings; tests and benchmarks reference the exe, which means
  the packaged library surface itself is only covered indirectly.
- `Api/` and `Middleware/` model an HTTP-style surface without a host; they are scaffolding
  for a future hosted service.

## Testing and benchmarks

- `tests/gpu-image-processing.Tests` - xUnit; golden-image fixtures (`fixtures/*.ppm|pgm`)
  exercise the CPU processor byte-exactly, plus service/pipeline/domain suites.
- `benchmarks/gpu-image-processing.Benchmarks` - BenchmarkDotNet suites for filter chains,
  batch processing and utility hot paths.
