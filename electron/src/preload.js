// Preload — contexto aislado, sin acceso a Node.js desde el renderer
const { contextBridge } = require('electron')

contextBridge.exposeInMainWorld('dyabooApp', {
  version: process.env.npm_package_version ?? '1.0.0',
  platform: process.platform,
})
