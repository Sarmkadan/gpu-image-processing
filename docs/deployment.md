# Deployment Guide

Complete guide for deploying GPU Image Processing in various environments.

## Table of Contents

- [Development Environment](#development-environment)
- [Docker Deployment](#docker-deployment)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Linux Server Deployment](#linux-server-deployment)
- [Windows Server Deployment](#windows-server-deployment)
- [Performance Tuning](#performance-tuning)
- [Monitoring & Logging](#monitoring--logging)

## Development Environment

### Local Setup

```bash
# Clone and build
git clone https://github.com/Sarmkadan/gpu-image-processing.git
cd gpu-image-processing

# Install dependencies
dotnet restore

# Build
dotnet build -c Debug

# Run
dotnet run
```

### With Docker Compose (Development)

```bash
docker-compose -f docker-compose.yml up
```

## Docker Deployment

### Building Docker Image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY . .
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /build/bin/Release/net10.0 .
ENTRYPOINT ["dotnet", "GpuImageProcessing.dll"]
```

### Build and Run

```bash
# Build image
docker build -t gpu-image-processing:latest .

# Run with GPU support (NVIDIA)
docker run --rm --gpus all \
  -v $(pwd)/images:/app/images \
  -v $(pwd)/output:/app/output \
  gpu-image-processing:latest

# Run without GPU (CPU fallback)
docker run --rm \
  -v $(pwd)/images:/app/images \
  -v $(pwd)/output:/app/output \
  gpu-image-processing:latest
```

### Docker Compose

```yaml
version: '3.8'
services:
  app:
    build: .
    image: gpu-image-processing:latest
    container_name: gpu-app
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ENABLE_GPU=true
      - MAX_PARALLEL_OPERATIONS=4
    volumes:
      - ./images:/app/images
      - ./output:/app/output
      - ./logs:/app/logs
    ports:
      - "8080:80"
    runtime: nvidia  # For GPU support
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
```

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster 1.24+
- NVIDIA GPU operator installed
- Docker registry access

### Deployment YAML

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gpu-image-processing
  labels:
    app: gpu-processing
spec:
  replicas: 2
  selector:
    matchLabels:
      app: gpu-processing
  template:
    metadata:
      labels:
        app: gpu-processing
    spec:
      containers:
      - name: app
        image: gpu-image-processing:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ENABLE_GPU
          value: "true"
        - name: MAX_PARALLEL_OPERATIONS
          value: "4"
        resources:
          requests:
            memory: "2Gi"
            nvidia.com/gpu: "1"
          limits:
            memory: "4Gi"
            nvidia.com/gpu: "1"
        volumeMounts:
        - name: images
          mountPath: /app/images
        - name: output
          mountPath: /app/output
        - name: logs
          mountPath: /app/logs
      volumes:
      - name: images
        persistentVolumeClaim:
          claimName: images-pvc
      - name: output
        persistentVolumeClaim:
          claimName: output-pvc
      - name: logs
        emptyDir: {}
```

### Deploy to Kubernetes

```bash
# Create namespaces and PVCs
kubectl create namespace gpu-processing

# Apply deployment
kubectl apply -f deployment.yaml -n gpu-processing

# Check status
kubectl get pods -n gpu-processing
kubectl logs -f deployment/gpu-image-processing -n gpu-processing
```

## Linux Server Deployment

### Prerequisites

```bash
# Ubuntu 22.04
sudo apt-get update
sudo apt-get install -y \
  dotnet-sdk-10.0 \
  dotnet-runtime-10.0 \
  nvidia-driver-535 \
  nvidia-cuda-toolkit \
  ocl-icd-opencl-dev
```

### Installation

```bash
# Create application directory
mkdir -p /opt/gpu-image-processing
cd /opt/gpu-image-processing

# Download release
wget https://github.com/Sarmkadan/gpu-image-processing/releases/download/v1.0.0/release.tar.gz
tar -xzf release.tar.gz

# Create systemd service
sudo tee /etc/systemd/system/gpu-processing.service > /dev/null <<EOF
[Unit]
Description=GPU Image Processing Service
After=network.target

[Service]
Type=simple
User=gpuprocessing
WorkingDirectory=/opt/gpu-image-processing
ExecStart=/usr/bin/dotnet GpuImageProcessing.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

# Create user
sudo useradd -r -s /bin/false gpuprocessing

# Set permissions
sudo chown -R gpuprocessing:gpuprocessing /opt/gpu-image-processing

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable gpu-processing
sudo systemctl start gpu-processing

# Check status
sudo systemctl status gpu-processing
```

### Monitor Service

```bash
# View logs
sudo journalctl -u gpu-processing -f

# Check resource usage
ps aux | grep GpuImageProcessing
nvidia-smi

# Restart service
sudo systemctl restart gpu-processing
```

## Windows Server Deployment

### Prerequisites

```powershell
# Install .NET 10.0 Runtime
# Download from https://dotnet.microsoft.com/download

# Install NVIDIA drivers
# Download from https://www.nvidia.com/Download/

# Install OpenCL headers
# Available in CUDA Toolkit
```

### Installation

```powershell
# Create directory
New-Item -ItemType Directory -Path "C:\gpu-image-processing"
cd C:\gpu-image-processing

# Download and extract release
Invoke-WebRequest -Uri "https://github.com/Sarmkadan/gpu-image-processing/releases/download/v1.0.0/release.zip" -OutFile "release.zip"
Expand-Archive -Path "release.zip" -DestinationPath "."

# Create Windows service
sc.exe create GpuProcessing binPath= "C:\gpu-image-processing\GpuImageProcessing.exe"

# Start service
Start-Service GpuProcessing

# Check status
Get-Service GpuProcessing
```

## Performance Tuning

### GPU Configuration

```csharp
// Optimal settings for high throughput
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.Processing.MaxParallelOperations = 8;
settings.Processing.BatchSize = 32;
settings.Processing.UseGPUAcceleration = true;
settings.Processing.Precision = "float16";  // Faster
settings.Cache.EnableDistributedCache = true;
settings.Cache.CacheTTLSeconds = 7200;      // 2 hours
```

### Memory Optimization

```csharp
// Conservative settings for limited memory
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.Processing.MaxParallelOperations = 2;
settings.Processing.BatchSize = 5;
settings.Cache.MaxCacheSize = 536870912;    // 512MB
settings.Processing.DebugMode = false;
```

### CPU Fallback

```csharp
settings.Device.AllowFallbackToCPU = true;
settings.Device.PreferredDeviceType = "GPU";

// Monitor fallback usage
var metrics = await perfService.GetMetricsAsync();
Console.WriteLine($"Using GPU: {metrics.IsUsingGpu}");
```

### Network Optimization

If using HTTP API:

```csharp
// Enable compression
settings.Middleware.EnableCompressionMiddleware = true;

// Set appropriate timeout
settings.Processing.TimeoutSeconds = 600;

// Rate limiting
settings.Middleware.RateLimitPerMinute = 100;
```

## Monitoring & Logging

### Application Logging

```csharp
// Configure logging
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.Logging.LogLevel = LogLevel.Information;
settings.Logging.EnableFileLogging = true;
settings.Logging.LogFilePath = "/var/log/gpu-processing/app.log";
```

### Health Checks

```csharp
// Enable background health checks
var healthCheckService = serviceProvider
    .GetRequiredService<HealthCheckService>();

var status = await healthCheckService.CheckHealthAsync();
Console.WriteLine($"GPU Available: {status.GpuAvailable}");
Console.WriteLine($"Memory OK: {status.MemoryOk}");
Console.WriteLine($"Disk Space: {status.DiskSpaceGB}GB");
```

### Performance Monitoring

```bash
# Monitor GPU usage
watch -n 1 nvidia-smi

# Monitor memory
free -h

# Monitor disk
df -h

# Monitor CPU
top -p $(pgrep -f GpuImageProcessing)
```

### Metrics Export

```csharp
// Export to Prometheus
var metricsPublisher = serviceProvider
    .GetRequiredService<MetricsPublisher>();

var metrics = await metricsPublisher.GetMetricsAsync();
// Metrics in Prometheus format:
// gpu_utilization_percent 45.5
// gpu_memory_used_mb 2048
// processing_queue_depth 12
```

### Log Aggregation

```bash
# Send logs to ELK Stack
sudo filebeat -c /etc/filebeat/filebeat.yml -e

# Or send to Splunk
export SPLUNK_HEC_URL="https://splunk.example.com:8088"
export SPLUNK_HEC_TOKEN="your-token"
```

### Alerting

```yaml
# Prometheus alert rules
groups:
  - name: gpu-processing
    rules:
    - alert: HighGpuUtilization
      expr: gpu_utilization_percent > 90
      for: 5m
      annotations:
        summary: "GPU utilization above 90%"
    
    - alert: ProcessingQueueDepth
      expr: processing_queue_depth > 50
      for: 10m
      annotations:
        summary: "Processing queue depth above 50"
    
    - alert: ServiceDown
      expr: up{job="gpu-processing"} == 0
      for: 1m
      annotations:
        summary: "GPU Processing Service is down"
```

## Troubleshooting Deployment

### GPU Not Available

```bash
# Check NVIDIA driver
nvidia-smi

# Check OpenCL support
clinfo

# Enable fallback to CPU
ENABLE_GPU=false dotnet GpuImageProcessing.dll
```

### Memory Issues

```bash
# Check memory usage
free -h

# Kill other processes
pkill -f non-essential-process

# Reduce batch size in settings
settings.Processing.BatchSize = 5;
```

### Performance Issues

```bash
# Check GPU clock speed
nvidia-smi -pm 1
nvidia-smi -lgc 1500  # Set lock clock

# Check thermal throttling
nvidia-smi dmon

# Enable performance mode
echo "performance" | sudo tee /sys/devices/system/cpu/cpu*/cpufreq/scaling_governor
```

### Service Not Starting

```bash
# Check logs
sudo journalctl -u gpu-processing -n 50

# Verify .NET installation
dotnet --version

# Test application locally
cd /opt/gpu-image-processing
dotnet GpuImageProcessing.dll
```

## Backup & Disaster Recovery

### Backup Strategy

```bash
# Daily backup of configuration and results
0 2 * * * tar -czf /backup/gpu-processing-$(date +%Y%m%d).tar.gz /opt/gpu-image-processing/

# Weekly backup to cloud
0 3 * * 0 aws s3 sync /backup s3://backups/gpu-processing/
```

### Restore from Backup

```bash
# List available backups
ls -la /backup/

# Restore
tar -xzf /backup/gpu-processing-20260501.tar.gz -C /

# Restart service
sudo systemctl restart gpu-processing
```

## Security Considerations

### Network Security

```csharp
// Enable HTTPS/SSL
settings.Middleware.EnableSslMiddleware = true;
settings.Server.CertificatePath = "/etc/ssl/certs/server.crt";
settings.Server.KeyPath = "/etc/ssl/private/server.key";
```

### Access Control

```csharp
// Enable authorization
settings.Middleware.EnableAuthorizationMiddleware = true;
settings.Security.RequireApiKey = true;
settings.Security.ApiKey = "your-secure-key";
```

### Rate Limiting

```csharp
settings.Middleware.EnableRateLimitingMiddleware = true;
settings.Middleware.RateLimitPerMinute = 60;
settings.Middleware.RateLimitPerHour = 1000;
```

For more information, see [Getting Started](getting-started.md) and [API Reference](api-reference.md).
