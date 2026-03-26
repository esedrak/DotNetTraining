.PHONY: build test run-hello run-bank-api run-bank-cli run-worker run-client \
        db-up infra-up infra-down db-migrate clean fmt lint help

## Build all projects
build:
	dotnet build DotNetTraining.sln

## Run all tests
test:
	dotnet test DotNetTraining.sln --logger "console;verbosity=normal"

## Run Hello console app
run-hello:
	dotnet run --project src/Hello

## Run Bank API server
run-bank-api:
	dotnet run --project src/Bank.Api

## Run Bank CLI
run-bank-cli:
	dotnet run --project src/Bank.Cli -- $(ARGS)

## Run Temporal worker
run-worker:
	dotnet run --project src/Temporal.Worker

## Run Temporal client
run-client:
	dotnet run --project src/Temporal.Client

## Start PostgreSQL only
db-up:
	docker compose up -d postgres

## Start all infrastructure (Postgres, Temporal, WireMock, Jaeger)
infra-up:
	docker compose up -d

## Stop all infrastructure
infra-down:
	docker compose down

## Run EF Core migrations
db-migrate:
	@echo "Waiting for PostgreSQL to be ready..."
	@until docker compose exec -T postgres pg_isready -U dotnettrainer -d dotnetbank > /dev/null 2>&1; do \
		echo "PostgreSQL is not ready yet, retrying in 1s..."; \
		sleep 1; \
	done
	dotnet ef database update --project src/Bank.Repository

## Format code
fmt:
	dotnet format DotNetTraining.sln

## Verify formatting (used in CI)
lint:
	dotnet format DotNetTraining.sln --verify-no-changes

## Clean build artifacts
clean:
	dotnet clean DotNetTraining.sln
	find . -type d \( -name bin -o -name obj \) -not -path "./.git/*" -exec rm -rf {} + 2>/dev/null; true

## Show this help
help:
	@grep -hE '^## .*|^[a-zA-Z_-]+:' $(MAKEFILE_LIST) | \
	  awk '/^## /{desc=$$0; sub(/^## /,"",desc); next} /^[a-zA-Z_-]+:/{printf "\033[36m%-20s\033[0m %s\n", $$1, desc; desc=""}'
	@echo ""
	@echo "\033[33mCommon Issues:\033[0m"
	@echo "  Port 5432 conflict: ensure no local PostgreSQL is running."
	@echo "  Docker not running: ensure Docker Desktop is active before 'make infra-up'."
