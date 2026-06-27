# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Dyaboo ERP** — MVP de modernización del sistema central de una empresa textil. Tres módulos críticos: PLM (ciclo de vida de producto), SAG (administración y costos), WMS (gestión de bodega).

## Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 8 (C#), Clean Architecture, EF Core 8, Npgsql |
| Base de datos | PostgreSQL 16 |
| Frontend | Next.js 14, App Router, TypeScript |
| Contrato API | Swagger/OpenAPI → `openapi-typescript` para generar tipos TS |
| Infraestructura local | Docker Compose (3 contenedores, red `dyaboo-network`) |

## Comandos de infraestructura

```bash
# Primera vez — copiar variables de entorno
cp .env.example .env

# Levantar todos los servicios
docker compose up --build

# Solo base de datos (útil para desarrollo local del backend)
docker compose up postgres

# Ver logs de un servicio específico
docker compose logs -f api
docker compose logs -f frontend

# Eliminar volúmenes (reset de BD)
docker compose down -v
```

## Desarrollo local del backend (sin Docker)

```bash
cd backend

# Restaurar dependencias
dotnet restore

# Ejecutar API (requiere postgres corriendo en puerto 5433)
dotnet run --project src/Dyaboo.WebAPI

# Tests
dotnet test
dotnet test tests/Dyaboo.Application.Tests   # un solo proyecto

# EF Core migrations
dotnet ef migrations add <NombreMigración> --project src/Dyaboo.Infrastructure --startup-project src/Dyaboo.WebAPI
dotnet ef database update --project src/Dyaboo.Infrastructure --startup-project src/Dyaboo.WebAPI
```

## Desarrollo local del frontend (sin Docker)

```bash
cd frontend

# Instalar dependencias
npm ci

# Dev server (apunta a API en localhost:8080)
npm run dev

# Generar tipos TypeScript desde OpenAPI (requiere API corriendo)
npm run generate-types   # script a definir: openapi-typescript http://localhost:8080/swagger/v1/swagger.json -o src/types/api-generated/index.ts

# Build de producción
npm run build
```

## Arquitectura Clean Architecture (backend)

```
Domain        → Entities, ValueObjects, Enums, Domain Events. Sin dependencias externas.
Application   → Use Cases (CQRS con MediatR), DTOs, interfaces de repositorios.
Infrastructure → Implementaciones: EF Core DbContext, repositorios, servicios externos.
WebAPI        → Controllers thin, middleware, extensiones de DI. Solo orquesta Application.
```

**Regla de dependencias:** `WebAPI → Application → Domain`. Infrastructure implementa interfaces definidas en Application/Domain. Nunca inversas.

## Módulos de negocio

- **PLM** (`/plm`): Referencias textiles con variaciones Talla × Color. Entidades: `ProductReference`, `Variant`, `SizeValueObject`, `ColorValueObject`.
- **SAG** (`/sag`): Cálculo de costos de producción y actualización de inventario financiero.
- **WMS** (`/wms`): Asignación de ubicaciones físicas (Pasillo/Estante/Nivel). Endpoint de alta velocidad — usar proyecciones EF Core, nunca `Include()` ciego.

## Red Docker interna

| Servicio | Host externo | Interno (red `dyaboo-network`) |
|---|---|---|
| postgres | `localhost:5433` | `postgres:5432` |
| api (.NET) | `localhost:8080` | `api:8080` |
| frontend | `localhost:3000` | — |

Los Server Components y Route Handlers de Next.js usan `API_INTERNAL_URL=http://api:8080`. El browser usa `NEXT_PUBLIC_API_URL=http://localhost:8080`.

## Nota importante Next.js

El `Dockerfile` del frontend usa `output: 'standalone'` de Next.js. Esta opción **debe estar configurada** en `next.config.ts` para que el build de Docker funcione:

```ts
// next.config.ts
const nextConfig = {
  output: 'standalone',
}
```