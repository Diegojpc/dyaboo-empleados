import type { Metadata, Viewport } from 'next'
import './globals.css'
import { ThemeProvider } from '@/components/ThemeProvider'

export const metadata: Metadata = {
  title: 'Dyaboo ERP',
  description: 'Sistema ERP interno — PLM, SAG y WMS',
  manifest: '/manifest.webmanifest',
  appleWebApp: {
    capable: true,
    title: 'Dyaboo ERP',
    statusBarStyle: 'black-translucent',
  },
  icons: {
    icon: [
      { url: '/icons/favicon-16.png', sizes: '16x16', type: 'image/png' },
      { url: '/icons/favicon-32.png', sizes: '32x32', type: 'image/png' },
    ],
    apple: '/icons/apple-touch-icon.png',
    shortcut: '/icons/favicon.ico',
  },
}

export const viewport: Viewport = {
  themeColor: '#108474',
  width: 'device-width',
  initialScale: 1,
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" suppressHydrationWarning>
      <head>
        {/* Anti-flash de tema */}
        <script dangerouslySetInnerHTML={{ __html: `
          try {
            const t = localStorage.getItem('dyaboo-theme') || 'dark';
            if (t === 'dark') document.documentElement.classList.add('dark');
          } catch(e) {}
        `}} />
        {/* Registro del Service Worker */}
        <script dangerouslySetInnerHTML={{ __html: `
          if ('serviceWorker' in navigator) {
            window.addEventListener('load', function() {
              navigator.serviceWorker.register('/sw.js')
                .catch(function(err) { console.warn('SW:', err); });
            });
          }
        `}} />
      </head>
      <body className="antialiased">
        <ThemeProvider>{children}</ThemeProvider>
      </body>
    </html>
  )
}
