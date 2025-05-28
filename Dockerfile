# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy project files
COPY *.csproj ./
COPY . .

# Build the application
RUN dotnet build -c Release

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Install OpenCL support
RUN apt-get update && apt-get install -y \
    ocl-icd-libopencl1 \
    ocl-icd-opencl-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directories for input/output
RUN mkdir -p /app/images /app/output /app/logs

# Set environment variables
ENV ENABLE_GPU=true
ENV LOG_LEVEL=Information

# Run the application
ENTRYPOINT ["dotnet", "gpu-image-processing.dll"]
