# Arquitectura Frontend (Next.js 15)

## Estructura de rutas — App Router

```mermaid
graph TD
    ROOT["src/app/"]

    subgraph AUTH_GROUP["(auth) — layout: solo fondo oscuro"]
        LOGIN["login/page.tsx\n/login"]
    end

    subgraph MAIN_GROUP["(main) — layout: Sidebar + header"]
        DASHBOARD["page.tsx\n/ (dashboard)"]
        PLM["plm/page.tsx\n/plm"]
        SAG["sag/page.tsx\n/sag"]
        WMS["wms/page.tsx\n/wms"]
    end

    LAYOUT_ROOT["layout.tsx\n(root — ThemeProvider, SW, meta PWA)"]

    ROOT --> LAYOUT_ROOT
    LAYOUT_ROOT --> AUTH_GROUP
    LAYOUT_ROOT --> MAIN_GROUP
```

Los **route groups** `(auth)` y `(main)` comparten el root layout pero tienen layouts propios independientes. Esto evita que el Sidebar y header aparezcan en la pantalla de login.

## Flujo de navegación y protección de rutas

```mermaid
flowchart TD
    REQ["Request del navegador"]
    MW["middleware.ts\n(Next.js Edge)"]
    TOKEN{¿JWT en\nlocalStorage?}
    LOGIN_PAGE["/login"]
    MAIN_APP["App principal\n(layout con Sidebar)"]
    API_CHECK["Client-side:\nchequea token al montar"]

    REQ --> MW
    MW --> TOKEN
    TOKEN -->|No tiene| LOGIN_PAGE
    TOKEN -->|Tiene| MAIN_APP
    MAIN_APP --> API_CHECK
    API_CHECK -->|Token expirado (401)| LOGIN_PAGE
```

> **Nota de seguridad**: El token se almacena en `localStorage`. Para el MVP interno con Electron y red LAN esto es aceptable; en producción web pública migrar a HttpOnly cookies (ver recomendaciones del documento de seguridad).

## Árbol de componentes

```mermaid
graph TD
    RL["RootLayout (layout.tsx)\n- ThemeProvider\n- Registra SW\n- Meta PWA"]

    RL --> ML["MainLayout (main/layout.tsx)\n- Sidebar\n- Slot children"]
    RL --> AL["AuthLayout (auth/layout.tsx)\n- Solo fondo oscuro"]

    ML --> DB["Dashboard (page.tsx)\n- KPI cards\n- Stats de BD"]
    ML --> PLM["PLM (plm/page.tsx)\n- Lista referencias\n- ImageGallery"]
    ML --> SAG["SAG (sag/page.tsx)\n- Inventario financiero\n- Cálculo de costos"]
    ML --> WMS["WMS (wms/page.tsx)\n- Estado de bodega\n- Asignación de stock"]

    AL --> LG["Login (login/page.tsx)\n- Form email+password\n- apiLogin()"]

    ML --> SB["Sidebar.tsx\n- Nav items por rol\n- Botón logout\n- ThemeToggle"]
    PLM --> IG["ImageGallery.tsx\n- Hero + thumbnails\n- Upload/Delete\n- Lightbox"]
```

## Sistema de temas (dark/light mode)

```mermaid
sequenceDiagram
    participant HTML as <html>
    participant SCRIPT as Anti-flash script
    participant TP as ThemeProvider
    participant USER as Usuario

    Note over HTML: Antes de que React hidrate:
    HTML->>SCRIPT: Lee localStorage('theme')
    SCRIPT->>HTML: Aplica class='dark' o 'light'\ninmediatamente (sin flash)

    Note over TP: React monta:
    TP->>TP: Sincroniza estado\ncon clase del DOM
    USER->>TP: Click en ThemeToggle
    TP->>HTML: Alterna class dark/light
    TP->>HTML: Guarda en localStorage
```

Tailwind usa `darkMode: 'class'`. Los colores del sistema son CSS custom properties en `globals.css`:

| Variable | Light | Dark |
|---|---|---|
| `--bg` | `#F0EFEB` | `#1A1918` |
| `--surface` | `#FFFFFF` | `#252321` |
| `--accent` | `#108474` | `#108474` |
| `--text` | `#110D0B` | `#F0EFEB` |
| `--muted` | `#7c7975` | `#7c7975` |
| `--border` | `#E5E2DC` | `#302E2C` |
| `--sidebar-bg` | `#473C38` | `#1A1918` |

## Capas de API client

```mermaid
graph LR
    PAGE["Página / Componente"]
    CLIENT["src/lib/api/client.ts\napiGet / apiPost / apiLogin\n(client-side fetch)"]
    SERVER["src/lib/api/server.ts\nserverFetch\n(Server Components)"]
    API_EXT["API Externa\nhttp://localhost:8080\n(desde browser)"]
    API_INT["API Interna\nhttp://api:8080\n(desde Next.js server)"]

    PAGE -->|Client components| CLIENT
    PAGE -->|Server components| SERVER
    CLIENT --> API_EXT
    SERVER --> API_INT
```

- `NEXT_PUBLIC_API_URL` → usado por el browser (client-side)
- `API_INTERNAL_URL` → usado por Server Components y Route Handlers (server-side)
- Server-side no tiene CORS: va directo de contenedor a contenedor en la red Docker

## Sesión y almacenamiento local

```typescript
// src/lib/auth/session.ts
saveSession({ token, userId, name, email, role })
  → localStorage('dyaboo_token') = token
  → localStorage('dyaboo_user')  = { userId, name, email, role }

getToken()   → string | null
getUser()    → { userId, name, email, role } | null
clearSession() → borra ambas claves → redirect /login
```

## PWA (Progressive Web App)

| Archivo | Propósito |
|---|---|
| `src/app/manifest.ts` | Genera `/manifest.webmanifest` en runtime (Next.js App Router) |
| `public/sw.js` | Service Worker — network-first para navegación; no intercepta API ni MinIO |
| `public/icons/` | icon-192.png, icon-512.png (maskable), apple-touch-icon.png, favicon.ico |
| `src/app/layout.tsx` | Registra el SW + metadata PWA (themeColor, appleWebApp) |

El SW es **network-first**: si hay red → respuesta fresca; si no → caché. Solo intercepta requests de navegación (`document`), no llamadas a la API ni imágenes de MinIO.

## Security headers (next.config.ts)

Todos los headers se aplican a `source: '/(.*)'` (todas las rutas):

| Header | Valor |
|---|---|
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `X-XSS-Protection` | `1; mode=block` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=(), payment=()` |
| `Strict-Transport-Security` | `max-age=63072000; includeSubDomains` |
| `Content-Security-Policy` | `default-src 'self'`; allowlist API + MinIO |

## Convenciones de estilos

- Tailwind utility classes para layout y espaciado
- CSS custom properties (`var(--accent)`) para colores del sistema (permite theme switching)
- `style={{ ... }}` inline para valores dinámicos (colores de variante, etc.)
- Sin CSS Modules ni styled-components
- `globals.css`: define custom properties, componentes base (`.card`, `.table-row`, `.page-header`)
- `font-size: 18px` en `html` — base para toda la tipografía relativa

## Dependencias principales

| Paquete | Uso |
|---|---|
| `next` 15.3 | Framework |
| `react` 19 | UI |
| `tailwindcss` 3 | Utilidades CSS |
| `typescript` 5 | Tipado |
| `openapi-typescript` | Genera tipos desde Swagger |
