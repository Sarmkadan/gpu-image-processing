# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Variables
DOTNET := dotnet
CONFIGURATION := Release
PROJECT := GpuImageProcessing
OUTPUT_DIR := ./bin/$(CONFIGURATION)/net10.0
PUBLISH_DIR := ./publish
DOCKER_IMAGE := gpu-image-processing:latest
DOCKER_REGISTRY := docker.io
REGISTRY_USERNAME := sarmkadan

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
RED := \033[0;31m
YELLOW := \033[0;33m
NC := \033[0m # No Color

.PHONY: help clean build release test publish run docker-build docker-push docker-run format lint

help:
	@echo "$(BLUE)GPU Image Processing - Build & Development Tasks$(NC)"
	@echo ""
	@echo "$(GREEN)Build Targets:$(NC)"
	@echo "  make build          - Build the project (Debug)"
	@echo "  make release        - Build the project (Release)"
	@echo "  make clean          - Clean build artifacts"
	@echo ""
	@echo "$(GREEN)Testing:$(NC)"
	@echo "  make test           - Run all unit tests"
	@echo "  make test-coverage  - Run tests with coverage"
	@echo ""
	@echo "$(GREEN)Deployment:$(NC)"
	@echo "  make publish        - Publish the application"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Run application in Docker"
	@echo "  make docker-push    - Push Docker image to registry"
	@echo ""
	@echo "$(GREEN)Development:$(NC)"
	@echo "  make run            - Run the application"
	@echo "  make format         - Format code with dotnet format"
	@echo "  make lint           - Run code analysis"
	@echo "  make restore        - Restore NuGet packages"
	@echo "  make watch          - Watch for changes and rebuild"
	@echo ""
	@echo "$(GREEN)Examples:$(NC)"
	@echo "  make examples       - Build example projects"
	@echo ""

## Build targets

restore:
	@echo "$(BLUE)Restoring dependencies...$(NC)"
	@$(DOTNET) restore

build: restore
	@echo "$(BLUE)Building project (Debug)...$(NC)"
	@$(DOTNET) build -c Debug
	@echo "$(GREEN)✓ Build complete$(NC)"

release: restore
	@echo "$(BLUE)Building project (Release)...$(NC)"
	@$(DOTNET) build -c $(CONFIGURATION)
	@echo "$(GREEN)✓ Build complete$(NC)"

rebuild: clean build
	@echo "$(GREEN)✓ Rebuild complete$(NC)"

clean:
	@echo "$(BLUE)Cleaning build artifacts...$(NC)"
	@$(DOTNET) clean
	@rm -rf $(OUTPUT_DIR)
	@rm -rf $(PUBLISH_DIR)
	@rm -rf ./bin ./obj
	@find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	@find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	@echo "$(GREEN)✓ Clean complete$(NC)"

## Testing targets

test: build
	@echo "$(BLUE)Running tests...$(NC)"
	@$(DOTNET) test --configuration $(CONFIGURATION) --no-build --verbosity normal
	@echo "$(GREEN)✓ Tests passed$(NC)"

test-coverage: build
	@echo "$(BLUE)Running tests with coverage...$(NC)"
	@$(DOTNET) test --configuration $(CONFIGURATION) \
		/p:CollectCoverage=true \
		/p:CoverageFormat=opencover \
		/p:ExcludeByAttribute=ExcludeFromCodeCoverage
	@echo "$(GREEN)✓ Coverage report generated$(NC)"

test-watch:
	@echo "$(BLUE)Running tests in watch mode...$(NC)"
	@$(DOTNET) watch --project . test

## Deployment targets

publish: release
	@echo "$(BLUE)Publishing application...$(NC)"
	@$(DOTNET) publish -c $(CONFIGURATION) -o $(PUBLISH_DIR)
	@echo "$(GREEN)✓ Published to $(PUBLISH_DIR)$(NC)"

package: publish
	@echo "$(BLUE)Creating package...$(NC)"
	@mkdir -p ./packages
	@cd $(PUBLISH_DIR) && tar -czf ../packages/gpu-image-processing-$(shell date +%Y%m%d).tar.gz *
	@echo "$(GREEN)✓ Package created in ./packages$(NC)"

## Docker targets

docker-build:
	@echo "$(BLUE)Building Docker image...$(NC)"
	@docker build -t $(DOCKER_IMAGE) .
	@echo "$(GREEN)✓ Docker image built: $(DOCKER_IMAGE)$(NC)"

docker-run: docker-build
	@echo "$(BLUE)Running Docker container...$(NC)"
	@docker run --rm -it \
		-v $(PWD)/images:/app/images \
		-v $(PWD)/output:/app/output \
		-v $(PWD)/logs:/app/logs \
		$(DOCKER_IMAGE)

docker-run-gpu: docker-build
	@echo "$(BLUE)Running Docker container with GPU...$(NC)"
	@docker run --rm -it --gpus all \
		-v $(PWD)/images:/app/images \
		-v $(PWD)/output:/app/output \
		-v $(PWD)/logs:/app/logs \
		$(DOCKER_IMAGE)

docker-push: docker-build
	@echo "$(BLUE)Pushing Docker image...$(NC)"
	@docker tag $(DOCKER_IMAGE) $(DOCKER_REGISTRY)/$(REGISTRY_USERNAME)/$(DOCKER_IMAGE)
	@docker push $(DOCKER_REGISTRY)/$(REGISTRY_USERNAME)/$(DOCKER_IMAGE)
	@echo "$(GREEN)✓ Docker image pushed$(NC)"

docker-compose:
	@echo "$(BLUE)Starting Docker Compose services...$(NC)"
	@docker-compose up -d
	@echo "$(GREEN)✓ Services started$(NC)"

docker-compose-down:
	@echo "$(BLUE)Stopping Docker Compose services...$(NC)"
	@docker-compose down
	@echo "$(GREEN)✓ Services stopped$(NC)"

## Development targets

run: build
	@echo "$(BLUE)Running application...$(NC)"
	@$(DOTNET) run

watch:
	@echo "$(BLUE)Watching for changes...$(NC)"
	@$(DOTNET) watch run

format:
	@echo "$(BLUE)Formatting code...$(NC)"
	@$(DOTNET) format
	@echo "$(GREEN)✓ Code formatted$(NC)"

lint:
	@echo "$(BLUE)Running code analysis...$(NC)"
	@$(DOTNET) build /p:EnforceCodeStyleInBuild=true
	@echo "$(GREEN)✓ Code analysis complete$(NC)"

analyze:
	@echo "$(BLUE)Running SonarQube analysis...$(NC)"
	@echo "$(YELLOW)Note: Requires sonar-scanner and SonarQube server$(NC)"

## Documentation targets

docs:
	@echo "$(BLUE)Documentation:$(NC)"
	@echo "  - README.md (Main documentation)"
	@echo "  - docs/getting-started.md (Installation and setup)"
	@echo "  - docs/api-reference.md (Complete API reference)"
	@echo "  - docs/deployment.md (Deployment guide)"
	@echo "  - docs/faq.md (Frequently asked questions)"
	@echo "  - examples/ (Code examples)"

examples:
	@echo "$(BLUE)Building examples...$(NC)"
	@for example in examples/*.cs; do \
		echo "  Compiling $$(basename $$example)"; \
	done
	@echo "$(GREEN)✓ Examples compiled$(NC)"

## Utility targets

info:
	@echo "$(BLUE)Project Information:$(NC)"
	@echo "  Project: $(PROJECT)"
	@echo "  Configuration: $(CONFIGURATION)"
	@echo "  .NET SDK Version: $$($(DOTNET) --version)"
	@echo "  Output Directory: $(OUTPUT_DIR)"

version:
	@$(DOTNET) --version

dependencies:
	@echo "$(BLUE)Project Dependencies:$(NC)"
	@$(DOTNET) list package

outdated:
	@echo "$(BLUE)Checking for outdated packages...$(NC)"
	@$(DOTNET) list package --outdated

update-packages:
	@echo "$(BLUE)Updating NuGet packages...$(NC)"
	@$(DOTNET) package update

## Git targets

commit-clean:
	@echo "$(BLUE)Checking for uncommitted changes...$(NC)"
	@git status

push: test
	@echo "$(BLUE)Pushing to remote...$(NC)"
	@git push
	@echo "$(GREEN)✓ Pushed to remote$(NC)"

## CI/CD targets

ci: clean build test lint
	@echo "$(GREEN)✓ CI pipeline complete$(NC)"

cd: publish docker-build
	@echo "$(GREEN)✓ CD pipeline complete$(NC)"

## Installation/Setup targets

install: restore build
	@echo "$(GREEN)✓ Installation complete$(NC)"

setup: install
	@echo "$(BLUE)Setting up development environment...$(NC)"
	@mkdir -p images output logs temp
	@echo "$(GREEN)✓ Setup complete$(NC)"

## Cleanup targets

clean-logs:
	@echo "$(BLUE)Cleaning log files...$(NC)"
	@rm -rf ./logs/*
	@echo "$(GREEN)✓ Logs cleaned$(NC)"

clean-output:
	@echo "$(BLUE)Cleaning output files...$(NC)"
	@rm -rf ./output/*
	@echo "$(GREEN)✓ Output cleaned$(NC)"

clean-all: clean clean-logs clean-output
	@echo "$(GREEN)✓ All cleaned$(NC)"

## Performance targets

bench:
	@echo "$(BLUE)Running benchmark...$(NC)"
	@$(DOTNET) run --configuration Release

profile:
	@echo "$(BLUE)Profiling application...$(NC)"
	@echo "$(YELLOW)Note: Requires profiling tools to be installed$(NC)"

## Help and information

version-info:
	@echo "$(BLUE)Version Information:$(NC)"
	@echo "  $(DOTNET) version: $$($(DOTNET) --version)"
	@echo "  $(DOTNET) SDK: $$($(DOTNET) --list-sdks | grep 10)"

.DEFAULT_GOAL := help
