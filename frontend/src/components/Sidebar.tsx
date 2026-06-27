'use client'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useSession } from '@/lib/auth/useSession'
import { clearSession, navItemsForRole } from '@/lib/auth/session'
import ThemeToggle from './ThemeToggle'

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
}

export default function Sidebar() {
  const { user } = useSession()
  const pathname = usePathname()
  const router = useRouter()

  if (!user) return null

  const navItems = navItemsForRole(user.role)

  const handleLogout = () => {
    clearSession()
    router.replace('/login')
  }

  return (
    <aside className="flex w-56 flex-col bg-slate-900 dark:bg-slate-950 text-slate-200 shrink-0 border-r border-slate-800">
      {/* Logo */}
      <div className="flex items-center gap-3 px-4 py-4 border-b border-slate-800">
        <Image
          src="/dyaboo-logo.png"
          alt="Dyaboo"
          width={36}
          height={36}
          className="rounded object-contain"
        />
        <div>
          <p className="font-semibold text-white text-sm leading-tight">Dyaboo</p>
          <p className="text-indigo-400 text-xs">Empleados</p>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex flex-col gap-0.5 p-2 flex-1">
        {navItems.map(({ href, label, icon }) => {
          const active = href === '/' ? pathname === '/' : pathname.startsWith(href)
          return (
            <Link
              key={href}
              href={href}
              className={`flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors
                ${active
                  ? 'bg-indigo-600 text-white'
                  : 'text-slate-300 hover:bg-slate-800 hover:text-white'}`}
            >
              <span>{icon}</span>
              {label}
            </Link>
          )
        })}
      </nav>

      {/* Footer: tema + usuario + logout */}
      <div className="border-t border-slate-800 p-3 space-y-3">
        <div className="flex items-center justify-between px-1">
          <span className="text-xs text-slate-500">Tema</span>
          <ThemeToggle />
        </div>
        <div className="px-1">
          <p className="text-xs font-medium text-white truncate">{user.name}</p>
          <p className="text-xs text-indigo-400">{ROLE_LABELS[user.role] ?? user.role}</p>
        </div>
        <button
          onClick={handleLogout}
          className="w-full text-left flex items-center gap-2 rounded-lg px-3 py-2 text-xs
                     text-slate-400 hover:bg-slate-800 hover:text-red-400 transition-colors"
        >
          <span>⎋</span> Cerrar sesión
        </button>
      </div>
    </aside>
  )
}
