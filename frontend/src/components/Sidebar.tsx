'use client'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useSession } from '@/lib/auth/useSession'
import { clearSession, navItemsForRole } from '@/lib/auth/session'
import ThemeToggle from './ThemeToggle'
import { useTheme } from './ThemeProvider'

const ROLE_LABELS: Record<string, string> = {
  Ceo:               'CEO',
  Socio:             'Socio',
  LiderPlm:          'Líder PLM',
  LiderProduccion:   'Líder Producción',
  LiderBodega:       'Líder Bodega',
  LiderDistribucion: 'Líder Distribución',
  Disenadora:        'Diseñadora',
  Vendedor:          'Vendedor',
  Operario:          'Operario',
  GestionHumana:     'Gestión Humana',
}

export default function Sidebar() {
  const { user }  = useSession()
  const { theme } = useTheme()
  const pathname  = usePathname()
  const router    = useRouter()
  const isDark    = theme === 'dark'

  if (!user) return null

  const navItems = navItemsForRole(user.role)

  const handleLogout = () => {
    clearSession()
    router.replace('/login')
  }

  return (
    <aside
      className="flex w-56 flex-col shrink-0 border-r transition-colors duration-200"
      style={{
        backgroundColor: 'var(--sidebar-bg)',
        borderColor:     'var(--sidebar-border)',
        color:           'var(--sidebar-text)',
      }}
    >
      {/* Logo */}
      <div
        className="flex items-center px-4 py-4"
        style={{ borderBottom: '1px solid var(--sidebar-border)' }}
      >
        <Image
          src="/dyaboo-logo.png"
          alt="Dyaboo"
          width={110}
          height={24}
          className={`object-contain ${isDark ? 'brightness-0 invert' : 'brightness-0'}`}
          priority
        />
      </div>

      {/* Nav */}
      <nav className="flex flex-col gap-0.5 p-2 flex-1">
        {navItems.map(({ href, label, icon }) => {
          const active = href === '/' ? pathname === '/' : pathname.startsWith(href)
          return (
            <Link
              key={href}
              href={href}
              style={active ? {
                backgroundColor: 'var(--accent)',
                color: '#ffffff',
              } : {
                color: 'var(--sidebar-muted)',
              }}
              className={`flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors
                ${!active && (isDark
                  ? 'hover:bg-white/5 hover:!text-[var(--sidebar-text)]'
                  : 'hover:bg-black/5 hover:!text-[var(--sidebar-text)]'
                )}`}
            >
              <span>{icon}</span>
              {label}
            </Link>
          )
        })}
      </nav>

      {/* Footer */}
      <div
        className="p-3 space-y-2"
        style={{ borderTop: '1px solid var(--sidebar-border)' }}
      >
        {/* Toggle de tema */}
        <div className="flex items-center justify-between px-2 py-1">
          <span className="text-xs" style={{ color: 'var(--sidebar-muted)' }}>Tema</span>
          <ThemeToggle />
        </div>

        {/* Info del usuario */}
        <div className="px-2 pb-1">
          <p className="text-xs font-medium truncate" style={{ color: 'var(--sidebar-text)' }}>
            {user.name}
          </p>
          <p className="text-xs" style={{ color: 'var(--accent)' }}>
            {ROLE_LABELS[user.role] ?? user.role}
          </p>
        </div>

        {/* Logout */}
        <button
          onClick={handleLogout}
          className="w-full text-left flex items-center gap-2 rounded-lg px-3 py-2 text-xs transition-colors hover:text-red-500"
          style={{ color: 'var(--sidebar-muted)' }}
        >
          <span>⎋</span> Cerrar sesión
        </button>
      </div>
    </aside>
  )
}
