# ===========================================================================
# Accessly — developer task runner
# ===========================================================================
# Run `make` or `make help` to list available targets.

SHELL := /bin/bash
export DOTNET_ROOT ?= /opt/homebrew/opt/dotnet/libexec

SOLUTION       := Accessly.slnx
API_PROJECT    := src/Accessly.Api/Accessly.Api.csproj
INFRA_PROJECT  := src/Accessly.Infrastructure/Accessly.Infrastructure.csproj
WEB_DIR        := src/Accessly.Web
COMPOSE        := docker compose

.DEFAULT_GOAL := help

.PHONY: help setup restore build dev test lint format migrate seed \
        docker-up docker-down logs clean web-install web-build web-test

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| sort \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}'

setup: restore web-install ## Install backend and frontend dependencies

restore: ## Restore .NET dependencies
	dotnet restore $(SOLUTION)

build: ## Build the .NET solution
	dotnet build $(SOLUTION) --configuration Release --no-restore

dev: ## Run the API locally (Development)
	dotnet run --project $(API_PROJECT)

test: ## Run the .NET test suite
	dotnet test $(SOLUTION) --configuration Release

lint: ## Verify C# formatting (no changes applied)
	dotnet format $(SOLUTION) --verify-no-changes

format: ## Apply C# formatting fixes
	dotnet format $(SOLUTION)

migrate: ## Apply EF Core migrations to the database
	dotnet ef database update --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

seed: ## Seed demo data (runs the API seeder)
	dotnet run --project $(API_PROJECT) -- --seed

web-install: ## Install Angular dependencies
	cd $(WEB_DIR) && npm install

web-build: ## Build the Angular app
	cd $(WEB_DIR) && npm run build

web-test: ## Run Angular unit tests
	cd $(WEB_DIR) && npm test -- --watch=false --browsers=ChromeHeadless

docker-up: ## Start the full local stack with Docker Compose
	$(COMPOSE) up -d --build

docker-down: ## Stop and remove the local stack
	$(COMPOSE) down

logs: ## Tail logs from the local stack
	$(COMPOSE) logs -f --tail=100

clean: ## Remove build artifacts
	dotnet clean $(SOLUTION) || true
	find . -type d -name bin -prune -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -prune -exec rm -rf {} + 2>/dev/null || true
