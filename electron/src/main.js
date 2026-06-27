const { app, BrowserWindow, shell, dialog, Menu } = require('electron')
const path  = require('path')
const fs    = require('fs')
const http  = require('http')

// ── Configuración ─────────────────────────────────────────────────────────────
// Lee config.json junto al ejecutable (o dev fallback)
function loadConfig() {
  const configPaths = [
    path.join(process.resourcesPath ?? '', 'config.json'),
    path.join(__dirname, '..', 'config.json'),
    path.join(__dirname, '..', '..', 'config.json'),
  ]
  for (const p of configPaths) {
    try {
      if (fs.existsSync(p)) return JSON.parse(fs.readFileSync(p, 'utf8'))
    } catch (_) {}
  }
  return {}
}

const cfg        = loadConfig()
const SERVER_URL = cfg.serverUrl ?? process.env.DYABOO_SERVER_URL ?? 'http://localhost:3000'
const APP_TITLE  = 'Dyaboo ERP'

// ── Ventana principal ──────────────────────────────────────────────────────────
let win

function createWindow() {
  win = new BrowserWindow({
    width:     1440,
    height:    900,
    minWidth:  1024,
    minHeight: 680,
    title:     APP_TITLE,
    icon:      path.join(__dirname, '..', 'icons', 'icon.png'),
    webPreferences: {
      preload:          path.join(__dirname, 'preload.js'),
      nodeIntegration:  false,
      contextIsolation: true,
      // Permite cargar contenido del servidor de la empresa
      webSecurity: true,
    },
    show:            false,
    backgroundColor: '#108474',
    autoHideMenuBar: true,
  })

  // Menú mínimo: solo recargar y salir
  Menu.setApplicationMenu(Menu.buildFromTemplate([
    {
      label: 'Dyaboo ERP',
      submenu: [
        { label: 'Recargar', accelerator: 'F5',      click: () => win.webContents.reload() },
        { label: 'Pantalla completa', accelerator: 'F11', click: () => win.setFullScreen(!win.isFullScreen()) },
        { type: 'separator' },
        { label: 'Salir', accelerator: 'Alt+F4',     click: () => app.quit() },
      ],
    },
  ]))

  // Abrir links externos en el navegador del sistema, no en Electron
  win.webContents.setWindowOpenHandler(({ url }) => {
    shell.openExternal(url)
    return { action: 'deny' }
  })

  win.once('ready-to-show', () => win.show())

  loadApp()
}

// ── Conexión al servidor ───────────────────────────────────────────────────────
let retries = 0
const MAX_RETRIES = 20

function loadApp() {
  win.loadFile(path.join(__dirname, '..', 'splash', 'index.html')).catch(() => {})

  checkServer()
}

function checkServer() {
  const url = new URL(SERVER_URL)
  const options = { hostname: url.hostname, port: url.port || 80, path: '/health', timeout: 3000 }

  const req = http.get(options, (res) => {
    if (res.statusCode < 500) {
      retries = 0
      win.loadURL(SERVER_URL)
    } else {
      retry()
    }
  })

  req.on('error', retry)
  req.on('timeout', () => { req.destroy(); retry() })
}

function retry() {
  retries++
  if (retries >= MAX_RETRIES) {
    dialog.showMessageBox(win, {
      type:    'error',
      title:   'Sin conexión',
      message: 'No se pudo conectar al servidor Dyaboo.',
      detail:  `Verifica que estés conectado a la red de la empresa.\nServidor: ${SERVER_URL}`,
      buttons: ['Reintentar', 'Salir'],
    }).then(({ response }) => {
      if (response === 0) { retries = 0; checkServer() }
      else app.quit()
    })
    return
  }
  setTimeout(checkServer, 1500)
}

// ── Ciclo de vida ──────────────────────────────────────────────────────────────
app.whenReady().then(createWindow)

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})
