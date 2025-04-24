#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -e

# Colors
BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}$1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "  $1"
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking prerequisites..."

    # Check dotnet
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed"
        echo "Download from: https://dotnet.microsoft.com/download"
        exit 1
    fi

    local dotnet_version=$(dotnet --version)
    print_info ".NET Version: $dotnet_version"

    # Check git
    if ! command -v git &> /dev/null; then
        print_warning "Git is not installed (optional)"
    else
        print_info "Git: $(git --version | cut -d' ' -f3)"
    fi

    # Check make
    if ! command -v make &> /dev/null; then
        print_warning "Make is not installed (optional)"
    fi

    print_success "Prerequisites check complete"
}

# Restore dependencies
restore() {
    print_header "Restoring dependencies..."
    dotnet restore
    print_success "Dependencies restored"
}

# Build project
build() {
    local config=${1:-Release}
    print_header "Building project (${config})..."
    dotnet build -c "$config"
    print_success "Build complete"
}

# Run tests
run_tests() {
    print_header "Running tests..."
    dotnet test --configuration Release --no-build
    print_success "Tests passed"
}

# Publish release
publish() {
    print_header "Publishing release build..."
    dotnet publish -c Release -o ./publish
    print_success "Published to ./publish"
}

# Build Docker image
build_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        return 1
    fi

    print_header "Building Docker image..."
    docker build -t gpu-image-processing:latest .
    print_success "Docker image built"
}

# Format code
format_code() {
    print_header "Formatting code..."
    dotnet format
    print_success "Code formatted"
}

# Run analysis
run_analysis() {
    print_header "Running code analysis..."
    dotnet build /p:EnforceCodeStyleInBuild=true --configuration Release
    print_success "Code analysis complete"
}

# Display usage
show_usage() {
    cat << EOF
${BLUE}GPU Image Processing - Build Script${NC}

Usage: ./build.sh [COMMAND]

Commands:
    check           Check prerequisites
    restore         Restore NuGet dependencies
    build           Build project (Debug)
    release         Build project (Release)
    test            Run unit tests
    publish         Publish release build
    docker          Build Docker image
    format          Format code
    analyze         Run code analysis
    clean           Clean build artifacts
    all             Run check, restore, build, test, publish (default)

Examples:
    ./build.sh                # Full build pipeline
    ./build.sh release        # Release build only
    ./build.sh test           # Run tests
    ./build.sh docker         # Build Docker image

EOF
}

# Clean build artifacts
clean() {
    print_header "Cleaning build artifacts..."
    dotnet clean
    rm -rf ./bin ./obj ./publish
    find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
    find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
    print_success "Clean complete"
}

# Full build pipeline
full_build() {
    check_prerequisites
    restore
    build Release
    run_tests
    publish
    print_success "Full build pipeline complete"
}

# Main script
main() {
    local command=${1:-all}

    case "$command" in
        check)
            check_prerequisites
            ;;
        restore)
            restore
            ;;
        build)
            restore
            build Debug
            ;;
        release)
            restore
            build Release
            ;;
        test)
            run_tests
            ;;
        publish)
            build Release
            publish
            ;;
        docker)
            build_docker
            ;;
        format)
            format_code
            ;;
        analyze)
            run_analysis
            ;;
        clean)
            clean
            ;;
        all)
            full_build
            ;;
        help|--help|-h)
            show_usage
            ;;
        *)
            print_error "Unknown command: $command"
            show_usage
            exit 1
            ;;
    esac
}

# Run main
main "$@"
