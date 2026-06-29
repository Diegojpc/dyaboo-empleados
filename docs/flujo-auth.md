# Flujo de Autenticación

## Visión general

El sistema usa **JWT Bearer (HS256)** sin refresh tokens. El token se almacena en `localStorage` del browser (simplificado para MVP interno).

## Flujo de login

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FE as Next.js Frontend
    participant MW as Middleware Next.js
    participant API as .NET WebAPI
    participant DB as PostgreSQL

    U->>FE: Ingresa email + contraseña
    FE->>API: POST /api/auth/login\n{email, password}
    API->>DB: SELECT * FROM Users\nWHERE Email = @email
    DB-->>API: User record
    API->>API: BCrypt.Verify(password, hash)
    alt Credenciales inválidas
        API-->>FE: 401 Unauthorized
        FE-->>U: "Credenciales incorrectas"
    else Credenciales válidas
        API->>API: GenerateToken(userId, email, role)\nHS256, exp: 8h
        API-->>FE: 200 { token, userId,\nname, email, role }
        FE->>FE: localStorage.setItem('token', ...)
        FE->>FE: localStorage.setItem('user', ...)
        FE-->>U: Redirect a /dashboard
    end
```

## Middleware de rutas (Next.js)

```mermaid
flowchart TD
    REQ["Request entrante"] --> CHK{¿Ruta protegida?}
    CHK -->|No /login| PASS["Pasa directo"]
    CHK -->|Sí| TOKEN{¿Tiene cookie\no header JWT?}
    TOKEN -->|No| LOGIN["Redirect a /login"]
    TOKEN -->|Sí| VALID{¿Token válido?}
    VALID -->|No / expirado| LOGIN
    VALID -->|Sí| RENDER["Renderiza la página"]
```

Las rutas públicas son solo `/login`. Todo lo demás requiere token.

## Validación en el backend

Cada endpoint protegido lleva `[Authorize]`. Los endpoints que requieren rol específico usan `[Authorize(Roles = "Ceo,Socio")]`.

```mermaid
flowchart LR
    HDR["Authorization: Bearer <token>"] --> MID["JwtBearerMiddleware\n(.NET)"]
    MID --> DECODE["Decode HS256\nVerify signature\nCheck exp"]
    DECODE --> CTX["HttpContext.User\nClaimsPrincipal"]
    CTX --> CTRL["Controller / Handler\naccede a claims"]
```

## Roles del sistema

```mermaid
graph TD
    CEO["👑 Ceo\nAcceso total"]
    SOCIO["🤝 Socio\nAcceso total (sin admin de usuarios)"]
    PLM["📐 LiderPlm\nPLM completo"]
    PROD["🏭 LiderProduccion\nSAG completo"]
    BOD["📦 LiderBodega\nWMS completo"]
    DIST["🚚 LiderDistribucion\nLectura WMS"]
    DIS["🎨 Disenadora\nPLM — solo lectura + subir fotos"]
    VEN["💼 Vendedor\nConsulta referencias (PLM read)"]
    OP["⚙️ Operario\nLectura básica WMS"]

    CEO --> SOCIO
    SOCIO --> PLM & PROD & BOD
    BOD --> DIST & OP
    PLM --> DIS & VEN
```

## Claims en el JWT

| Claim | Valor |
|---|---|
| `sub` | GUID del usuario |
| `email` | email del usuario |
| `name` | nombre completo |
| `role` | nombre del enum `UserRole` |
| `iat` | issued at (Unix timestamp) |
| `exp` | expiry — 8 horas desde emisión |
