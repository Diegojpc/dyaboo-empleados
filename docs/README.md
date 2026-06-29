# Dyaboo ERP — Documentación Técnica

Sistema ERP de modernización para empresa textil. Cubre tres módulos críticos del ciclo productivo: PLM (ciclo de vida del producto), SAG (administración y costos) y WMS (gestión de bodega).

## Índice

| Documento | Contenido |
|---|---|
| [arquitectura.md](./arquitectura.md) | Stack, capas Clean Architecture, dependencias entre proyectos |
| [infraestructura.md](./infraestructura.md) | Servicios Docker, red interna, volúmenes, puertos |
| [base-de-datos.md](./base-de-datos.md) | Esquema de tablas, relaciones, migraciones EF Core |
| [flujo-auth.md](./flujo-auth.md) | Autenticación JWT, roles, middleware de rutas |
| [flujo-plm.md](./flujo-plm.md) | Módulo PLM: referencias, variantes, galería de imágenes |
| [flujo-sag.md](./flujo-sag.md) | Módulo SAG: cálculo de costos, inventario financiero |
| [flujo-wms.md](./flujo-wms.md) | Módulo WMS: asignación de stock, estado de bodega |
| [flujo-imagenes.md](./flujo-imagenes.md) | Pipeline completo de imágenes con MinIO |
| [frontend.md](./frontend.md) | Arquitectura Next.js, route groups, temas, componentes |
| [desktop.md](./desktop.md) | App de escritorio Electron para Windows |

## Resumen rápido del stack

```
Browser / Electron ──► Next.js 15 (App Router, TypeScript)
                              │
                              ▼ HTTP REST + JWT
                    .NET 8 WebAPI (Clean Architecture)
                              │
               ┌──────────────┼──────────────┐
               ▼              ▼              ▼
         PostgreSQL 16     MinIO S3      (futuro: Redis)
```

## Credenciales de desarrollo

| Usuario | Email | Contraseña | Rol |
|---|---|---|---|
| CEO Dyaboo | ceo@dyaboo.com | dyaboo2024 | Ceo |
| Socio | socio@dyaboo.com | dyaboo2024 | Socio |
| Líder PLM | plm@dyaboo.com | dyaboo2024 | LiderPlm |
| Líder Producción | produccion@dyaboo.com | dyaboo2024 | LiderProduccion |
| Líder Bodega | bodega@dyaboo.com | dyaboo2024 | LiderBodega |
| Diseñadora | disenadora@dyaboo.com | dyaboo2024 | Disenadora |
| Vendedor | vendedor@dyaboo.com | dyaboo2024 | Vendedor |
