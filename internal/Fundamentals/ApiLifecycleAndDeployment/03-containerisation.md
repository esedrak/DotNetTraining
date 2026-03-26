# Containerising .NET APIs

Docker best practices for .NET applications — minimal images, fast builds, production-ready.

---

## Why .NET Containers are Lean

.NET publishes a self-contained single file. The runtime is not installed on the OS.

| Runtime | Image Size |
| :--- | :--- |
| Python | ~900 MB |
| Java / JVM | ~500 MB |
| Node.js | ~400 MB |
| .NET (alpine) | ~100–150 MB |
| .NET (chiseled) | ~50–80 MB |

---

## Multi-Stage Dockerfile

```dockerfile
# Stage 1: Build — SDK image includes the compiler and all tools
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore (cached layer — only re-runs when .csproj changes)
COPY src/Bank.Api/Bank.Api.csproj src/Bank.Api/
COPY src/Bank.Domain/Bank.Domain.csproj src/Bank.Domain/
COPY src/Bank.Service/Bank.Service.csproj src/Bank.Service/
COPY src/Bank.Repository/Bank.Repository.csproj src/Bank.Repository/
RUN dotnet restore src/Bank.Api/Bank.Api.csproj

# Copy source and publish
COPY . .
RUN dotnet publish src/Bank.Api/Bank.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:PublishSingleFile=false  # keep multi-file for Docker layer caching

# Stage 2: Runtime — tiny image, no SDK, no compiler
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bank.Api.dll"]
```

Key points:
- **Stage 1 (SDK)**: full build environment, ~800 MB, **discarded after build**
- **Stage 2 (aspnet:alpine)**: runtime only, ~100 MB, **this is what runs in production**
- SDK never ships to production — the attack surface is minimal

---

## Chiseled Ubuntu (Smallest Possible)

Microsoft's "chiseled" images remove everything except what .NET needs:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-jammy-chiseled AS runtime
# ~50 MB — no shell, no package manager, no system utilities
# attacker has nothing to pivot from if they get code execution
```

---

## Image Tagging Strategy

Always tag with an **immutable semver tag** AND `latest`.

```bash
# Build
docker build -t myregistry.azurecr.io/bank-api:1.4.2 .
docker tag myregistry.azurecr.io/bank-api:1.4.2 myregistry.azurecr.io/bank-api:latest

# Push both
docker push myregistry.azurecr.io/bank-api:1.4.2
docker push myregistry.azurecr.io/bank-api:latest
```

**Never** use `latest` in production Kubernetes/ECS task definitions — `latest` is mutable and makes rollbacks unpredictable. Pin to `1.4.2`.

---

## Build-Time Version Metadata

Inject version, commit, and build time at publish time via `AssemblyInformationalVersion` or environment variables.

```dockerfile
ARG VERSION=dev
ARG COMMIT=unknown
ARG BUILD_TIME=unknown

RUN dotnet publish ... \
    -p:Version=$VERSION \
    -p:InformationalVersion=$VERSION+$COMMIT
```

Expose at `/buildinfo`:

```csharp
app.MapGet("/buildinfo", () => new
{
    Version = typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTimeOffset.UtcNow,
}).AllowAnonymous();
```

---

## CI/CD Pipeline

```yaml
# .github/workflows/build.yml
jobs:
  docker:
    steps:
      - name: Run tests
        run: dotnet test --no-build -c Release

      - name: Build image
        run: docker build -t bank-api:${{ github.sha }} .

      - name: Scan for CVEs
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: bank-api:${{ github.sha }}
          exit-code: 1
          severity: CRITICAL,HIGH

      - name: Push to registry
        run: |
          docker tag bank-api:${{ github.sha }} registry/bank-api:${{ github.ref_name }}
          docker push registry/bank-api:${{ github.ref_name }}
          docker tag bank-api:${{ github.sha }} registry/bank-api:latest
          docker push registry/bank-api:latest
```

---

## Security Best Practices

- [ ] Non-root user in container (shown above)
- [ ] Read-only root filesystem where possible
- [ ] No secrets in the image (use env vars / Azure Key Vault at runtime)
- [ ] Scan image for CVEs before push (Trivy, Snyk)
- [ ] Use Alpine or Chiseled for minimal attack surface
- [ ] Pin base image to a specific digest in production

---

## Further Reading

- [.NET Docker Hub images](https://hub.docker.com/_/microsoft-dotnet)
- [Chiseled Ubuntu containers](https://devblogs.microsoft.com/dotnet/dotnet-6-is-now-in-ubuntu-2204/)
- [Multi-stage Docker builds](https://docs.docker.com/build/building/multi-stage/)
