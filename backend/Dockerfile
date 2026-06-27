# ── Stage 1: Restore de dependencias (capa cacheada por NuGet) ──────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["src/Dyaboo.Domain/Dyaboo.Domain.csproj",           "src/Dyaboo.Domain/"]
COPY ["src/Dyaboo.Application/Dyaboo.Application.csproj", "src/Dyaboo.Application/"]
COPY ["src/Dyaboo.Infrastructure/Dyaboo.Infrastructure.csproj", "src/Dyaboo.Infrastructure/"]
COPY ["src/Dyaboo.WebAPI/Dyaboo.WebAPI.csproj",           "src/Dyaboo.WebAPI/"]

RUN dotnet restore "src/Dyaboo.WebAPI/Dyaboo.WebAPI.csproj" --locked-mode

# ── Stage 2: Build + Publish ──────────────────────────────────────────────
FROM restore AS build
COPY . .
RUN dotnet publish "src/Dyaboo.WebAPI/Dyaboo.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 3: Runtime mínimo (sin SDK) ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Usuario no-root para reducir superficie de ataque
RUN addgroup --system --gid 1001 dotnet \
 && adduser  --system --uid 1001 --ingroup dotnet appuser
USER appuser

COPY --from=build --chown=appuser:dotnet /app/publish .
ENTRYPOINT ["dotnet", "Dyaboo.WebAPI.dll"]
