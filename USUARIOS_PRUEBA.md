# Usuarios de Prueba — Dyaboo ERP

> **Contraseña de todos los usuarios:** `dyaboo2024`

## Usuarios disponibles

| Rol | Correo | Módulos que puede ver |
|---|---|---|
| **CEO** | `ceo@dyaboo.com` | Todo el sistema |
| **Socio** | `socio@dyaboo.com` | Todo el sistema |
| **Líder PLM** | `plm@dyaboo.com` | Dashboard, PLM |
| **Líder Producción** | `produccion@dyaboo.com` | Dashboard, SAG, Producción |
| **Líder Bodega** | `bodega@dyaboo.com` | Dashboard, WMS |
| **Líder Distribución** | `distribucion@dyaboo.com` | Dashboard, Distribución |
| **Diseñadora** | `disenadora@dyaboo.com` | Dashboard, PLM |
| **Vendedor** | `vendedor@dyaboo.com` | Dashboard, PLM (correría) |
| **Operario** | `operario@dyaboo.com` | Dashboard, WMS |

## Permisos por endpoint

| Endpoint | Roles autorizados |
|---|---|
| `POST /api/auth/login` | Público |
| `GET /api/auth/me` | Cualquier rol autenticado |
| `GET/POST /api/plm/*` | Ceo, Socio, LiderPlm, Disenadora, Vendedor |
| `GET/POST /api/sag/*` | Ceo, Socio, LiderProduccion |
| `GET/POST /api/wms/*` | Ceo, Socio, LiderBodega, Operario |

## Notas

- Los usuarios se crean automáticamente al iniciar la aplicación por primera vez (`UserSeeder`).
- Las contraseñas se almacenan con hash `BCrypt` (salt rounds: 11).
- Los tokens JWT expiran a las **8 horas**.
- Para cambiar contraseñas en producción, modificar `UserSeeder.cs` o implementar el módulo de gestión de usuarios.
