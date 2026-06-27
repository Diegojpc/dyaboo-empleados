import type { MetadataRoute } from 'next'

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: 'Dyaboo ERP',
    short_name: 'Dyaboo',
    description: 'Sistema ERP — PLM, SAG y WMS',
    start_url: '/',
    display: 'standalone',
    orientation: 'landscape',
    background_color: '#108474',
    theme_color: '#108474',
    categories: ['business', 'productivity'],
    icons: [
      { src: '/icons/icon-192.png', sizes: '192x192', type: 'image/png', purpose: 'any' },
      { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'any' },
      { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
    ],
  }
}
