# ── Stage 1: restore ─────────────────────────────────────────────────────────
# Copy only project manifests first so Docker can cache the restore layer
# independently of source-file changes.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/EdTech.SheetIntegrator.Domain/EdTech.SheetIntegrator.Domain.csproj \
     src/EdTech.SheetIntegrator.Domain/
COPY src/EdTech.SheetIntegrator.Application/EdTech.SheetIntegrator.Application.csproj \
     src/EdTech.SheetIntegrator.Application/
COPY src/EdTech.SheetIntegrator.Infrastructure/EdTech.SheetIntegrator.Infrastructure.csproj \
     src/EdTech.SheetIntegrator.Infrastructure/
COPY src/EdTech.SheetIntegrator.Api/EdTech.SheetIntegrator.Api.csproj \
     src/EdTech.SheetIntegrator.Api/

RUN dotnet restore src/EdTech.SheetIntegrator.Api/EdTech.SheetIntegrator.Api.csproj

# ── Stage 2: publish ──────────────────────────────────────────────────────────
FROM restore AS publish
COPY src/ src/

RUN dotnet publish src/EdTech.SheetIntegrator.Api/EdTech.SheetIntegrator.Api.csproj \
    --configuration Release \
    --no-restore \
    -p:UseAppHost=false \
    -o /publish

# ── Stage 3: runtime ──────────────────────────────────────────────────────────
# noble-chiseled = Ubuntu 24.04 LTS stripped to only what .NET needs:
# no shell, no package manager, no OS utilities. Smallest Microsoft base image.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS runtime

WORKDIR /app
EXPOSE 8080

# Run as the non-root user pre-created by the chiseled image (UID 1654).
USER $APP_UID

COPY --from=publish /publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# NOTE: the noble-chiseled image intentionally omits shell and wget/curl,
# so a CMD-style HEALTHCHECK is not possible here.
# In production use an external probe: Kubernetes liveness probe, AWS ECS
# health check, or the load balancer — all targeting GET /health/live.
# docker-compose overrides this with its own healthcheck on the host side.

ENTRYPOINT ["dotnet", "EdTech.SheetIntegrator.Api.dll"]
