# GPU Image Processing - Architecture Overview

## Project Structure

```
gpu-image-processing/
├── Core/
│   ├── Models/              (8 domain models)
│   │   ├── Image.cs
│   │   ├── Filter.cs
│   │   ├── FilterParameter.cs
│   │   ├── Transform.cs
│   │   ├── ProcessingJob.cs
│   │   ├── ProcessingResult.cs
│   │   ├── DeviceInfo.cs
│   │   └── ProcessingProfile.cs
│   ├── Services/            (5 business logic services)
│   │   ├── ImageProcessingService.cs
│   │   ├── FilterService.cs
│   │   ├── TransformService.cs
│   │   ├── BatchProcessingService.cs
│   │   └── DeviceService.cs
│   ├── Repository/          (5 data access classes)
│   │   ├── IRepository.cs (generic interface)
│   │   ├── GenericRepository.cs
│   │   ├── ImageRepository.cs
│   │   ├── JobRepository.cs
│   │   └── ResultRepository.cs
│   ├── Configuration/       (2 configuration classes)
│   │   ├── ApplicationSettings.cs
│   │   └── DependencyInjectionSetup.cs
│   ├── Constants/           (4 enums and constants)
│   │   ├── FilterTypes.cs
│   │   ├── TransformTypes.cs
│   │   ├── ImageFormats.cs
│   │   └── ProcessingStatus.cs
│   └── Exceptions/          (2 custom exception types)
│       ├── ImageProcessingException.cs
│       └── OpenCLException.cs
├── Program.cs               (entry point)
├── gpu-image-processing.csproj
├── LICENSE                  (MIT)
├── .gitignore
├── README.md
└── ARCHITECTURE.md
```

## Statistics

- **Total Lines of Code**: 4,317
- **C# Source Files**: 27
- **Domain Models**: 8 classes
- **Services**: 5 classes with full business logic
- **Repository Classes**: 5 (generic + specialized)
- **Custom Exceptions**: 2 custom exception types
- **Configuration Classes**: 2
- **Constants/Enums**: 4 types

## Layer Architecture

### 1. Domain Models (Core/Models/)
Represents the core business entities:
- **Image**: Metadata and properties for image assets
- **Filter**: Image filtering operations with parameters
- **FilterParameter**: Configurable parameters for filters
- **Transform**: Geometric and color space transformations
- **ProcessingJob**: Batch processing job tracking
- **ProcessingResult**: Result of individual image processing
- **DeviceInfo**: GPU/CPU device information
- **ProcessingProfile**: Configuration profiles for optimization

### 2. Data Access Layer (Core/Repository/)
Generic CRUD operations with specialized queries:
- **IRepository<T>**: Generic interface for data access
- **GenericRepository<T>**: In-memory implementation with thread safety
- **ImageRepository**: Specialized queries for images (format, size, processed status)
- **JobRepository**: Job queries (status, date range, statistics)
- **ResultRepository**: Result analytics (performance, filters used, compression ratios)

### 3. Service Layer (Core/Services/)
Business logic and orchestration:
- **ImageProcessingService**: Core processing operations, image registration, validation
- **FilterService**: Filter management, kernel code generation, OpenCL integration
- **TransformService**: Transform pipeline management, parameter control
- **BatchProcessingService**: Job execution, progress tracking, queue management
- **DeviceService**: GPU device detection, capability scoring, device selection

### 4. Configuration & Dependency Injection
- **ApplicationSettings**: Hierarchical configuration (OpenCL, processing, storage, performance)
- **DependencyInjectionSetup**: Complete service registration and initialization

### 5. Constants & Enums
- **FilterTypes**: Gaussian, Bilateral, Median, Sobel, Canny, Laplacian, etc.
- **TransformTypes**: Rotate, Resize, ColorSpace, Normalize, Flip, Crop, Brightness, Contrast
- **ImageFormats**: JPEG, PNG, BMP, TIFF, WebP, GIF, AVIF, RAW
- **ProcessingStatus**: Pending, Running, Completed, Failed, Cancelled, Paused

### 6. Exception Handling
Custom exception hierarchy for detailed error reporting:
- **ImageProcessingException**: Base exception with error codes
- **ImageFileException**: File I/O errors
- **InvalidImageException**: Image validation failures
- **OpenCLException**: GPU/OpenCL related errors
- **DeviceInitializationException**: Device detection failures
- **KernelCompilationException**: Kernel compilation with logs

## Key Features

### Image Processing
- Register images with metadata
- Validate image properties
- Track processing history
- Support for multiple image formats

### Filter System
- 11+ filter types with OpenCL kernels
- Configurable parameters per filter
- Parameter validation and clamping
- Kernel code generation and caching

### Transform Pipeline
- Chainable transforms with execution order
- Parameter management (rotation angle, scale, etc.)
- Active/inactive toggle
- Pipeline serialization

### Batch Processing
- Job queuing and execution
- Progress tracking (percentage, ETA)
- Pause/resume capability
- Cancellation support
- Success/failure statistics

### Device Management
- Automatic GPU/CPU detection
- Capability scoring
- Memory management
- Device selection and switching

### Processing Profiles
- Speed-optimized (float16, parallel=8)
- Quality-optimized (float32, parallel=2)
- Balanced (default)
- Custom profile creation

### Statistics & Monitoring
- Image statistics (total size, aspect ratio, pixel count)
- Job statistics (completion time, processing rate)
- Filter usage analytics
- Device capabilities summary

## Design Patterns

1. **Repository Pattern**: Generic CRUD operations with specialized repositories
2. **Service Layer**: Business logic separated from data access
3. **Dependency Injection**: Loose coupling with Microsoft.Extensions.DependencyInjection
4. **Factory Pattern**: Static factory methods for creating entities (Filter.CreatePredefined)
5. **Builder Pattern**: Fluent configuration for profiles and jobs
6. **Observer Pattern**: Progress tracking through properties
7. **Strategy Pattern**: Different processing profiles
8. **Template Method**: Generic repository with specialization

## Technologies

- **.NET 10.0**: Latest C# language features
- **OpenCL 1.2+**: GPU acceleration via Cloo
- **ImageSharp**: Image I/O operations
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Async/Await**: Non-blocking operations

## Code Quality

- XML documentation on all public members
- Custom exception types for specific errors
- Thread-safe repository operations
- Validation at system boundaries
- No hardcoded values in service logic
- Configurable through ApplicationSettings

---

**Author**: Vladyslav Zaiets
**License**: MIT
