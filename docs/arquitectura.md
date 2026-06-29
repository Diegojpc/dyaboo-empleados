# Arquitectura del Sistema

## Visión general — C4 Nivel 2 (Contenedores)

```mermaid
graph TB
    subgraph USUARIOS["👥 Usuarios"]
        CEO["CEO / Socio"]
        PLM_USER["Líder PLM / Diseñadora"]
        WMS_USER["Líder Bodega"]
        SAG_USER["Líder Producción"]
    end

    subgraph CLIENTE["💻 Clientes"]
        BROWSER["Navegador Web\nChrome / Edge"]
        ELECTRON["App Escritorio\nElectron (Windows)"]
    end

    subgraph SERVIDOR["🖥️ Servidor Empresa (Docker)"]
        NEXT["Frontend\nNext.js 15\nPort 3000"]
        API["Backend API\n.NET 8 WebAPI\nPort 8080"]
        PG["Base de Datos\nPostgreSQL 16\nPort 5433"]
        MINIO["Object Storage\nMinIO S3\nPort 9000"]
    end

    CEO & PLM_USER & WMS_USER & SAG_USER --> BROWSER
    CEO & PLM_USER & WMS_USER & SAG_USER --> ELECTRON

    BROWSER -->|"HTTP :3000"| NEXT
    ELECTRON -->|"HTTP :3000"| NEXT

    NEXT -->|"HTTP :8080 (interno: api:8080)"| API
    API -->|"TCP :5432 (interno)"| PG
    API -->|"S3 API :9000 (interno: minio:9000)"| MINIO

    BROWSER -->|"HTTP :9000 (imágenes directas)"| MINIO
    ELECTRON -->|"HTTP :9000 (imágenes directas)"| MINIO
```

## Clean Architecture — Capas del Backend

```mermaid
graph BT
    subgraph WEBAPI["WebAPI (.NET 8)"]
        CTRL["Controllers\n(thin — solo orquesta)"]
        MW["Middleware\n(JWT, CORS, Health)"]
        DI["DI Extensions"]
    end

    subgraph APPLICATION["Application Layer"]
        CMD["Commands / Queries\n(CQRS con MediatR)"]
        HDL["Handlers"]
        IFACE["Interfaces\n(IApplicationDbContext\nIStorageService\nIJwtService)"]
        DTO["DTOs / Records de resultado"]
    end

    subgraph DOMAIN["Domain Layer"]
        ENT["Entities\n(ProductReference, Variant\nProductImage, User\nWarehouseLocation, etc.)"]
        VO["Value Objects\n(Size, Color\nWarehouseLocationCode)"]
        ENUM["Enums\n(ProductCategory\nUserRole\nProductionOrderStatus)"]
        BE["BaseEntity\n(Id: Guid, CreatedAt)"]
    end

    subgraph INFRA["Infrastructure Layer"]
        DBCTX["DyabooDbContext\n(EF Core 8 + Npgsql)"]
        MINIO_SVC["MinioStorageService"]
        JWT_SVC["JwtService"]
        PWD["PasswordHasher"]
        SEED["Seeders\n(UserSeeder\nWarehouseSeeder)"]
        CONF["EF Configurations\n(Fluent API por entidad)"]
    end

    WEBAPI -->|"usa"| APPLICATION
    APPLICATION -->|"usa"| DOMAIN
    INFRA -->|"implementa interfaces de"| APPLICATION
    INFRA -->|"usa"| DOMAIN
    WEBAPI -->|"registra"| INFRA

    style DOMAIN fill:#e8f5f3,stroke:#108474
    style APPLICATION fill:#f0efeb,stroke:#473c38
    style INFRA fill:#eee6de,stroke:#473c38
    style WEBAPI fill:#e5d3c4,stroke:#473c38
```

## Regla de dependencias

```
WebAPI → Application → Domain
Infrastructure → Application (implementa interfaces)
Infrastructure → Domain (usa entidades)
```

**Nunca:** Domain → Application, Domain → Infrastructure, Application → Infrastructure, Application → WebAPI.

## Módulos de negocio y sus features

```mermaid
graph LR
    subgraph PLM["📐 PLM"]
        PLM1["CreateProductReference\n(Command)"]
        PLM2["GetProductReferences\n(Query)"]
        PLM3["UploadImage\n(Controller directo)"]
        PLM4["DeleteImage\n(Controller directo)"]
    end

    subgraph SAG["💰 SAG"]
        SAG1["CalculateProductionCost\n(Command)"]
        SAG2["GetFinancialInventory\n(Query)"]
    end

    subgraph WMS["📦 WMS"]
        WMS1["AssignStock\n(Command)"]
        WMS2["GetWarehouseStatus\n(Query)"]
    end

    subgraph AUTH["🔐 Auth"]
        AUTH1["Login\n(Command)"]
    end
```

## Stack tecnológico completo

| Capa | Tecnología | Versión |
|---|---|---|
| Runtime backend | .NET / ASP.NET Core | 8.0 |
| ORM | Entity Framework Core + Npgsql | 8.x |
| Mediator | MediatR | 12.x |
| Base de datos | PostgreSQL | 16 |
| Object storage | MinIO | RELEASE.2025 |
| SDK MinIO .NET | Minio | 6.0.4 |
| Runtime frontend | Node.js | 20 LTS |
| Framework UI | Next.js (App Router) | 15.3 |
| Lenguaje frontend | TypeScript | 5.x |
| Estilos | Tailwind CSS | 3.x |
| App escritorio | Electron | 31.x |
| Empaquetador | electron-builder | 24.x |
| Contenedores | Docker + Compose | v2 |
| Auth | JWT Bearer (HS256) | — |
