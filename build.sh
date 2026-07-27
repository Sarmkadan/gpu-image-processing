#!/usr/bin/env bash
# build.sh – builds and tests the GPU Image Processing .NET solution.
# This script is invoked by the wrapper script in the sql-index-advisor
# repository. It assumes the .NET solution files are located in this
# directory (or sub‑directories) and that `dotnet` is installed.

set -euo pipefail

# Determine the directory where this script resides (the repository root)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Building GPU Image Processing project ==="
# Build all projects under the repository root
dotnet build "$SCRIPT_DIR"

echo "=== Running tests ==="
# Run all tests without rebuilding (the build step already compiled the code)
dotnet test "$SCRIPT_DIR" --no-build
