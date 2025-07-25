# HealthCheckWorker

A lightweight worker component responsible for performing periodic health checks on GPU processing services. It runs asynchronously in the background, validates service responsiveness, and exposes a simple name identifier for diagnostics.

## API

### `HealthCheckWorker`
Initializes a new instance of the `HealthCheckWorker` class. The worker is not started automatically; call `StartAsync` to begin health checks.

### `GetName`
