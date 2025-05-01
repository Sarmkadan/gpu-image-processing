# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-10-11

### Added
- Add compute shader pipeline with automatic workgroup optimization
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [0.9.0] - 2025-07-14

### Added
- `HealthCheckService` with GPU availability and memory pressure checks
- `PerformanceMonitoringService` with per-operation timing and throughput metrics
- `MetricsPublisher` for emitting counters to an external endpoint
- `TelemetryService` for structured trace collection
- `AsyncTaskQueue` for fire-and-forget background operations

### Changed
- `DeviceService` now exposes `GetDeviceCapabilitiesAsync` returning driver and extension details
- Processing results include `DurationMs`, `MemoryUsedMB`, and `GpuUtilizationPct` fields

### Fixed
- `HealthCheckWorker` could block the host shutdown on slow GPU probes
- Metrics timestamp drift on systems with non-monotonic clocks

## [0.8.0] - 2025-06-23

### Added
- `BackgroundWorkers`: `CacheMaintenanceWorker`, `HealthCheckWorker`, `JobProcessingWorker`, `MetricsAggregationWorker`
- `HttpImageClient` for downloading images from remote URLs with retry logic
- `WebhookHandler` for posting processing-complete notifications
- `DatabaseConnectionPool` integration stub
- `RemoteImageService` for federated image retrieval
- `NotificationService` for in-process alerts

### Changed
- `BatchProcessingService` now delegates to `JobProcessingWorker` for background execution
- Configuration validation moved to startup to fail fast on bad settings

### Fixed
- `HttpImageClient` did not dispose `HttpClient` between retries
- `WebhookHandler` silently swallowed non-2xx responses

## [0.7.0] - 2025-06-02

### Added
- Distributed caching via `DistributedCache` with configurable TTL
- `ProcessingCache` for in-process result memoization
- Result formatters: `JsonResultFormatter`, `CsvResultFormatter`, `XmlResultFormatter`, `HtmlResultFormatter`, `MarkdownResultFormatter`, `TextResultFormatter`
- `IResultFormatter` interface for custom formatter plugins

### Changed
- `ImageProcessingService.ProcessImageAsync` checks the cache before dispatching to GPU
- Cache keys include filter and transform parameter hashes to avoid stale hits

### Fixed
- XML formatter produced malformed output when result contained null fields
- CSV formatter did not escape commas in file paths

## [0.6.0] - 2025-05-19

### Added
- Domain event system: `EventAggregator`, `EventPublisher`, `DomainEvents`, `ProcessingEvents`
- Middleware pipeline: `IProcessingMiddleware`, `ProcessingPipeline`, `MiddlewareContext`
- Built-in middleware: `LoggingMiddleware`, `ErrorHandlingMiddleware`, `CompressionMiddleware`, `RateLimitingMiddleware`, `AuthorizationMiddleware`
- `ImageProcessingController` REST endpoints for image registration and job submission
- `RequestValidator` with input sanitization and size guards

### Changed
- Processing pipeline now runs through the middleware chain before reaching GPU kernels
- `OpenCLException` carries the native error code alongside the message

### Fixed
- Events were published before the result was persisted, causing consumers to read stale state
- Rate limiter window did not reset correctly across midnight UTC

## [0.5.0] - 2025-05-05

### Added
- `ApplicationSettings` with nested `ProcessingSettings`, `StorageSettings`, `CacheSettings`, `DeviceSettings`, `SecuritySettings`, `BatchSettings`
- `ConfigurationValidator` with `CreateDefaultSettings()` factory
- `DependencyInjectionSetup` for wiring all services in one call
- `appsettings.json` template with documented fields
- CLI subsystem: `CliParser`, `CommandDispatcher`, `CommandHandler`, `InteractiveShell`
- CLI commands: `ProcessImageCommand`, `BatchCommand`, `FilterCommand`, `DeviceCommand`, `HelpCommand`, `VersionCommand`

### Changed
- All services accept `ApplicationSettings` instead of individual constructor parameters
- Default GPU timeout raised from 30 s to 300 s

### Fixed
- CLI parser crashed on unrecognized flags instead of printing usage
- Device selection was not persisted between CLI invocations

## [0.4.0] - 2025-04-21

### Added
- `BatchProcessingService` with job queuing and progress tracking
- `ProcessingJob` model with `Status`, `TotalCount`, `ProcessedCount`, `FailureCount`
- `JobRepository` and `ResultRepository` for job persistence
- `ProcessingProfile` model supporting speed, quality, and balanced presets
- `ProcessingStatus` constants: Pending, Running, Completed, Failed, Cancelled, Paused

### Changed
- `ImageProcessingService.ProcessImageAsync` accepts a `profileId` to select the active profile
- Batch jobs now report per-image failure reasons rather than aborting on first error

### Fixed
- Jobs stuck in `Running` state when the processing thread threw an unhandled exception
- Batch size of 1 triggered a divide-by-zero in the progress calculation

## [0.3.0] - 2025-04-07

### Added
- Geometric transforms: `Resize`, `Rotate`, `AffineTransform`, `ColorSpaceConversion`, `Normalization`, `HistogramEqualization`
- `TransformService` with `CreateTransformAsync` and `UpdateTransformParametersAsync`
- `Transform` and `TransformType` models
- `TransformTypes` constants class
- Parameter composition: transforms and filters can be chained in a single `ProcessImageAsync` call

### Changed
- `ImageProcessingService` now accepts both filter and transform ID lists
- GPU kernel dispatch order: transforms applied before filters

### Fixed
- Rotation by 180° produced a mirrored image due to wrong matrix handedness
- Color space conversion to LAB clipped values outside the expected range

## [0.2.0] - 2025-03-24

### Added
- `FilterService` with `CreateFilterAsync`, `UpdateFilterParametersAsync`, `DeleteFilterAsync`
- Additional filters: `BilateralFilter`, `MedianFilter`, `SobelEdgeDetection`, `CannyEdgeDetection`, `MorphologicalOperation`
- `FilterParameter` model for typed parameter storage
- `FilterTypes` and `ImageFormats` constants
- `GenericRepository<T>`, `IRepository<T>`, `ImageRepository`, `FilterRepository`
- `DeviceService` with `GetAvailableDevicesAsync` and `SelectDeviceAsync`
- `DeviceInfo` model exposing `GlobalMemoryMB`, `ComputeUnits`, `ClockFrequencyMHz`
- Custom exceptions: `ImageProcessingException`, `OpenCLException`

### Changed
- Gaussian blur kernel now accepts arbitrary odd kernel sizes (previously fixed at 5×5)
- Device detection falls back to CPU automatically when no OpenCL GPU is found

### Fixed
- Bilateral filter produced NaN values for pixels at image boundaries
- Median filter kernel was applied without padding, cropping the output by one pixel per edge

## [0.1.0] - 2025-03-10

### Added
- Initial project scaffold targeting .NET 10.0
- OpenCL device enumeration via `Silk.NET.OpenCL`
- `Image` domain model with format, dimensions, and file path
- `ImageProcessingService` with `RegisterImageAsync` and basic `ProcessImageAsync`
- `GaussianBlurFilter` as the first GPU-accelerated operation
- `ProcessingResult` model with output path and status
- `Filter` and `FilterType` models
- MIT license, `.gitignore`, `.editorconfig`

---

**Changelog maintained by**: Vladyslav Zaiets | https://sarmkadan.com
