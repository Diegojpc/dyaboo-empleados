// Service Worker — Dyaboo ERP
// Network-first: el ERP siempre necesita datos frescos del servidor
const CACHE = 'dyaboo-v1'
const SHELL = ['/', '/login']

self.addEventListener('install', e => {
  e.waitUntil(
    caches.open(CACHE).then(c => c.addAll(SHELL)).then(() => self.skipWaiting())
  )
})

self.addEventListener('activate', e => {
  e.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k)))
    ).then(() => self.clients.claim())
  )
})

self.addEventListener('fetch', e => {
  const { request } = e
  // Solo interceptar navegación (páginas); dejar pasar API, MinIO y assets
  if (request.mode !== 'navigate') return

  e.respondWith(
    fetch(request)
      .catch(() => caches.match('/') || caches.match(request))
  )
})
