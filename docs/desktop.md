# App de Escritorio — Electron (Windows)

## Concepto

Electron es un **thin client** — no empaqueta el backend ni la base de datos. Abre una ventana de navegador apuntando al servidor Next.js de la empresa. Los empleados reciben un `.exe` que se comporta como app nativa pero consume el servidor central.

```mermaid
graph LR
    EXE["Dyaboo ERP.exe\n(Electron — 75 MB)"]
    CFG["config.json\n{serverUrl}"]
    NEXT["Servidor Next.js\nhttp://192.168.x.x:3000"]
    API["API .NET 8\n:8080"]
    DB["PostgreSQL\n:5433"]
    MINIO["MinIO\n:9000"]

    EXE -->|lee| CFG
    EXE -->|abre URL| NEXT
    NEXT --> API
    API --> DB
    API --> MINIO
```

**Prerequisito**: El empleado debe estar conectado a la red local de la empresa (LAN o VPN). Sin red, la app muestra el diálogo "Sin conexión".

## Flujo de arranque

```mermaid
sequenceDiagram
    participant OS as Windows
    participant MAIN as main.js (Electron)
    participant SPLASH as Splash Window
    participant SERVER as Servidor Next.js
    participant APP as BrowserWindow

    OS->>MAIN: Usuario abre el .exe
    MAIN->>MAIN: Lee config.json → serverUrl
    MAIN->>SPLASH: Muestra splash screen\n(logo + loading dots)

    loop Cada 1.5 segundos (máx 20 intentos = 30s)
        MAIN->>SERVER: GET {serverUrl}/health\n(HTTP simple, no fetch)
        alt Responde 200
            SERVER-->>MAIN: OK
            MAIN->>SPLASH: Cierra splash
            MAIN->>APP: Crea BrowserWindow (1440×900)\nabre {serverUrl}
        else Timeout / error
            Note over MAIN: Reintenta...
        end
    end

    alt 20 intentos sin respuesta
        MAIN->>OS: Dialog: "Sin conexión al servidor"\nBotones: Reintentar | Salir
    end
```

## Estructura de archivos

```
electron/
├── src/
│   ├── main.js          ← Proceso principal: ventana, splash, polling
│   └── preload.js       ← contextBridge (seguridad: no expone Node.js al renderer)
├── splash/
│   └── index.html       ← Pantalla de carga (HTML puro, sin dependencias)
├── icons/
│   └── icon.ico         ← Multi-size ICO (16,32,48,64,128,256 px)
├── config.json          ← { "serverUrl": "http://localhost:3000" }
├── package.json         ← electron-builder config + scripts
└── .gitignore           ← dist/, node_modules/
```

## Configuración antes de distribuir

Editar `electron/config.json` con la IP real del servidor:

```json
{
  "serverUrl": "http://192.168.1.100:3000"
}
```

Luego reconstruir:
```bash
cd electron
npm run build        # NSIS installer + portable (requiere Wine en Linux)
npm run build:dir    # Solo directorio desempaquetado (sin Wine)
```

## Targets de distribución

| Target | Archivo | Tamaño | Uso |
|---|---|---|---|
| NSIS installer | `Dyaboo ERP Setup 1.0.0.exe` | ~75 MB | Instalación con acceso directo |
| Portable | `Dyaboo ERP 1.0.0.exe` | ~75 MB | Sin instalación, ejecutar directo |

Ambos en `electron/dist/` tras el build.

## Opciones del menú interno

| Atajo | Acción |
|---|---|
| `F5` | Recargar la ventana |
| `F11` | Pantalla completa |
| `Alt+F4` | Cerrar la app |

Los enlaces externos (fuera de `serverUrl`) se abren en el browser del sistema, no en la app.

## Seguridad del proceso Electron

```mermaid
graph TD
    MAIN["Proceso Principal (main.js)\nAcceso completo a Node.js y OS"]
    RENDERER["Renderer (Next.js en BrowserWindow)\nSandboxed — NO tiene acceso a Node.js"]
    PRELOAD["preload.js\ncontextBridge — puente seguro"]

    MAIN -->|"contextBridge.exposeInMainWorld\n'dyabooApp': { version, platform }"| PRELOAD
    PRELOAD --> RENDERER

    note1["nodeIntegration: false\ncontextIsolation: true\n→ El renderer no puede\naceder al sistema de archivos,\nred nativa, ni módulos Node"]
```

Las opciones de seguridad de la `BrowserWindow`:

```javascript
webPreferences: {
    nodeIntegration:  false,  // renderer no puede usar require()
    contextIsolation: true,   // preload en contexto separado
    sandbox:          true,   // sandbox Chromium completo
    preload:          path.join(__dirname, 'preload.js'),
}
```

## Construir en Linux para Windows (cross-compilation)

```bash
# Requisitos para NSIS installer
sudo dpkg --add-architecture i386
sudo apt-get install wine64 wine32

# Omitir firma de código (sin certificado disponible)
export CSC_IDENTITY_AUTO_DISCOVERY=false

cd electron
npm install
npm run build        # produce installer + portable en dist/
npm run build:dir    # solo directorio, sin Wine necesario
```

## Próximos pasos para distribución empresarial

1. **Actualizar `config.json`** con IP/hostname real del servidor (`192.168.x.x` o DNS interno)
2. **Generar instalador final**: `npm run build` en la máquina con Wine
3. **Distribuir** el `Dyaboo ERP Setup 1.0.0.exe` a cada estación de trabajo
4. **Futuro** (opcional): firma con certificado de código para evitar el aviso de Windows SmartScreen
