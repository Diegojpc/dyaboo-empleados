# Infraestructura Docker

## Mapa de servicios y red

```mermaid
graph TB
    subgraph HOST["🖥️ Máquina Host"]
        subgraph DOCKER["Red: dyaboo-network (bridge)"]
            PG["dyaboo-postgres\nPostgreSQL 16-alpine\nInterno: postgres:5432"]
            MINIO["dyaboo-minio\nMinIO latest\nInterno: minio:9000\nConsola: minio:9001"]
            API["dyaboo-api\n.NET 8 ASP.NET\nInterno: api:8080"]
            FE["dyaboo-frontend\nNext.js 15 standalone\nInterno: frontend:3000"]
        end

        PORT1["localhost:5433"] -->|"mapea a"| PG
        PORT2["localhost:9000"] -->|"mapea a"| MINIO
        PORT3["localhost:9001"] -->|"mapea a (consola)"| MINIO
        PORT4["localhost:8080"] -->|"mapea a"| API
        PORT5["localhost:3000"] -->|"mapea a"| FE
    end

    PG -->|"healthcheck: pg_isready"| API
    MINIO -->|"healthcheck: curl /health/live"| API
    API -->|"healthcheck: curl /health"| FE

    VOL1[("postgres-data\nvolumen persistente")] --- PG
    VOL2[("minio-data\nvolumen persistente")] --- MINIO
```

## Orden de arranque (depends_on)

```mermaid
sequenceDiagram
    participant D as Docker Compose
    participant PG as PostgreSQL
    participant MN as MinIO
    participant API as .NET API
    participant FE as Next.js

    D->>PG: start
    D->>MN: start
    PG-->>D: healthy (pg_isready ok)
    MN-->>D: healthy (HTTP 200 /health/live)
    D->>API: start (ambos dependientes healthy)
    Note over API: Ejecuta migraciones EF Core
    Note over API: Siembra usuarios y bodega
    Note over API: Inicializa bucket MinIO
    API-->>D: healthy (HTTP 200 /health)
    D->>FE: start
    FE-->>D: ready
```

## Variables de entorno

### PostgreSQL
| Variable | Valor por defecto |
|---|---|
| `POSTGRES_DB` | `dyaboo_db` |
| `POSTGRES_USER` | `dyaboo_user` |
| `POSTGRES_PASSWORD` | `CAMBIAR_EN_PRODUCCION` |

### MinIO
| Variable | Valor por defecto |
|---|---|
| `MINIO_ROOT_USER` | `dyaboo_minio` |
| `MINIO_ROOT_PASSWORD` | `CAMBIAR_EN_PRODUCCION` |

### API (.NET)
| Variable | Descripción |
|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión PostgreSQL interna |
| `Minio__Endpoint` | `minio:9000` (nombre de servicio Docker) |
| `Minio__AccessKey` | Usuario MinIO |
| `Minio__SecretKey` | Contraseña MinIO |
| `Minio__Bucket` | `dyaboo-assets` |
| `Minio__PublicUrl` | `http://localhost:9000` (URL pública desde el browser) |
| `Minio__UseSSL` | `false` |

### Frontend (Next.js)
| Variable | Descripción |
|---|---|
| `NEXT_PUBLIC_API_URL` | `http://localhost:8080` — usada por el browser |
| `NEXT_PUBLIC_MINIO_URL` | `http://localhost:9000` — usada por el browser |
| `API_INTERNAL_URL` | `http://api:8080` — usada por Server Components |

## Comandos principales

```bash
# Levantar todo (primera vez o tras cambios de código)
docker compose up --build

# Solo base de datos (para desarrollo local del backend)
docker compose up postgres

# Ver logs en tiempo real de un servicio
docker compose logs -f api
docker compose logs -f frontend

# Detener sin borrar datos
docker compose down

# Detener Y BORRAR todos los volúmenes (reset completo de BD e imágenes)
docker compose down -v

# Reconstruir ignorando caché (cuando hay problemas de capas cacheadas)
docker compose build --no-cache
```

## Build multi-stage del backend

```mermaid
graph LR
    S1["Stage: restore\nSDK image\nCopia .csproj y ejecuta\ndotnet restore"]
    S2["Stage: build\nCopia código fuente\ndotnet publish -c Release"]
    S3["Stage: final\nASP.NET runtime image\n~220 MB (sin SDK)\nCopia /app/publish"]

    S1 --> S2 --> S3
```

**Optimización de caché:** Los archivos `.csproj` se copian antes que el código fuente. Si solo cambia código (no dependencias NuGet), el `dotnet restore` usa la capa cacheada y el build es ~10s más rápido.
