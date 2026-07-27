#!/usr/bin/env bash
# build.sh – wrapper script for the sql-index-advisor repository.
# This repository does not contain its own .NET project; the actual
# GPU Image Processing solution lives in the sibling directory
# ../gpu-image-processing. This wrapper forwards the build commands
# to the real build script there.

set -euo pipefail

# Determine the directory where this script resides
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Path to the actual GPU Image Processing repository
GPU_REPO_ROOT="${SCRIPT_DIR}/../gpu-image-processing"

# Verify that the target repository exists
if [[ ! -d "$GPU_REPO_ROOT" ]]; then
    echo "Error: GPU Image Processing repository not found at $GPU_REPO_ROOT"
    exit 1
fi

# Path to the real build script inside the GPU repo
REAL_BUILD_SCRIPT="${GPU_REPO_ROOT}/build.sh"

# Verify that the real build script exists and is executable
if [[ ! -x "$REAL_BUILD_SCRIPT" ]]; then
    echo "Error: Real build script not found or not executable at $REAL_BUILD_SCRIPT"
    exit 1
fi

# Forward all arguments to the real build script
exec "$REAL_BUILD_SCRIPT" "$@"
