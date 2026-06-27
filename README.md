# Dyaboo Empleados

Sistema ERP interno para la gestión de la empresa textil Dyaboo. Cubre el ciclo de vida completo de las prendas: diseño, correría, producción, bodega y distribución.

## Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 8 (C#), Clean Architecture, EF Core 8, PostgreSQL 16 |
| Frontend | Next.js 15, TypeScript, Tailwind CSS |
| Infraestructura | Docker Compose (3 contenedores) |

## Requisitos para correr localmente

| Herramienta | Versión mínima | Instalación |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 | `winget install Microsoft.DotNet.SDK.8` |
| [Node.js](https://nodejs.org/) | 18 LTS | `winget install OpenJS.NodeJS.LTS` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x | Descarga en docker.com |
| [Docker Compose](https://docs.docker.com/compose/) | v2 | Incluido en Docker Desktop |

> **Nota:** `uv` es un gestor de paquetes para Python — este proyecto no usa Python, por lo que no aplica. Los equivalentes aquí son `dotnet restore` (NuGet) y `npm install` (Node).

## Inicio rápido — Docker (recomendado)

```bash
# 1. Clonar el repositorio
git clone https://github.com/Diegojpc/dyaboo-empleados.git
cd dyaboo-empleados

# 2. Configurar variables de entorno
cp .env.example .env
# Editar .env con la contraseña deseada para PostgreSQL

# 3. Levantar todos los servicios
docker compose up --build
```

Los tres servicios quedan disponibles en:
- **Frontend:** http://localhost:3000
- **API:** http://localhost:8080
- **Swagger:** http://localhost:8080/swagger
- **PostgreSQL:** localhost:5433

## Inicio rápido — Desarrollo local (sin Docker)

### Base de datos

```bash
# Solo PostgreSQL en Docker
docker compose up postgres -d
```

### Backend (.NET)

```bash
cd backend

# Restaurar dependencias
dotnet restore

# Ejecutar API (requiere PostgreSQL en puerto 5433)
ASPNETCORE_URLS="http://localhost:8080" dotnet run --project src/Dyaboo.WebAPI
```

### Frontend (Next.js)

```bash
cd frontend

# Instalar dependencias
npm install

# Crear archivo de variables de entorno
echo "NEXT_PUBLIC_API_URL=http://localhost:8080" > .env.local

# Servidor de desarrollo
npm run dev
```

## Módulos

| Módulo | Ruta | Descripción |
|---|---|---|
| PLM | `/plm` | Ciclo de vida del producto: diseño, muestras, correría |
| SAG | `/sag` | Costos de producción e inventario financiero |
| WMS | `/wms` | Bodega: ubicaciones, asignación de stock |
| Producción | `/produccion` | Órdenes de corte y confección |
| Distribución | `/distribucion` | Clientes internos y externos, pedidos |

## Usuarios de prueba

Ver [`USUARIOS_PRUEBA.md`](./USUARIOS_PRUEBA.md) para la lista completa de usuarios y sus permisos.

Contraseña de todos los usuarios de prueba: `dyaboo2024`

## Arquitectura del backend

```
Domain        → Entidades, Value Objects, Enums. Sin dependencias externas.
Application   → Casos de uso (CQRS con MediatR), interfaces de repositorios.
Infrastructure → EF Core DbContext, repositorios, JWT, BCrypt.
WebAPI        → Controllers delgados, middleware, configuración DI.
```

Regla estricta: `WebAPI → Application → Domain`. Infrastructure implementa interfaces de Application.

## Migraciones de base de datos

```bash
cd backend

# Crear nueva migración
~/.dotnet/tools/dotnet-ef migrations add NombreMigracion \
  --project src/Dyaboo.Infrastructure \
  --startup-project src/Dyaboo.WebAPI

# Aplicar migraciones
~/.dotnet/tools/dotnet-ef database update \
  --project src/Dyaboo.Infrastructure \
  --startup-project src/Dyaboo.WebAPI
```

Las migraciones se aplican automáticamente al iniciar la aplicación.

## Variables de entorno

| Variable | Descripción | Ejemplo |
|---|---|---|
| `POSTGRES_USER` | Usuario de PostgreSQL | `dyaboo_user` |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL | `mi_password_seguro` |
| `POSTGRES_DB` | Nombre de la base de datos | `dyaboo_db` |
| `NEXT_PUBLIC_API_URL` | URL de la API (browser) | `http://localhost:8080` |
| `API_INTERNAL_URL` | URL de la API (Next.js server) | `http://api:8080` |
