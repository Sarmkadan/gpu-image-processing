# Architecture

This file used to describe an early version of the project (the original `Core/`-only
layout, ~27 source files). The codebase has grown far past that, so the architecture
documentation now lives in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) and covers the
current solution:

- the four projects in `gpu-image-processing.sln` and what each one compiles
- the library core in `src/` (domain, OpenCL device management, compute-shader pipeline,
  CPU fallback, PPM/PGM imaging, batch processing)
- the application shell in the root folders (CLI, API facade, middleware, events, workers)
- data flow, extension points, and known limitations

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
