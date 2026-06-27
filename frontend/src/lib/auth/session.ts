export interface SessionUser {
  userId: string
  name: string
  email: string
  role: string
  token: string
}

const COOKIE_TOKEN = 'dyaboo-token'
const COOKIE_USER  = 'dyaboo-user'

export function saveSession(user: SessionUser) {
  const expires = new Date(Date.now() + 8 * 60 * 60 * 1000).toUTCString()
  document.cookie = `${COOKIE_TOKEN}=${user.token}; path=/; expires=${expires}; SameSite=Lax`
  document.cookie = `${COOKIE_USER}=${encodeURIComponent(JSON.stringify({
    userId: user.userId, name: user.name, email: user.email, role: user.role,
  }))}; path=/; expires=${expires}; SameSite=Lax`
}

export function clearSession() {
  document.cookie = `${COOKIE_TOKEN}=; path=/; max-age=0`
  document.cookie = `${COOKIE_USER}=; path=/; max-age=0`
}

export function getToken(): string | null {
  return getCookie(COOKIE_TOKEN)
}

export function getSessionUser(): Omit<SessionUser, 'token'> | null {
  const raw = getCookie(COOKIE_USER)
  if (!raw) return null
  try { return JSON.parse(decodeURIComponent(raw)) } catch { return null }
}

function getCookie(name: string): string | null {
  if (typeof document === 'undefined') return null
  const match = document.cookie.match(new RegExp(`(?:^|; )${name}=([^;]*)`))
  return match ? decodeURIComponent(match[1]) : null
}

// Permisos por rol
const ROLE_NAV: Record<string, string[]> = {
  Ceo:               ['/', '/plm', '/sag', '/wms', '/produccion', '/distribucion'],
  Socio:             ['/', '/plm', '/sag', '/wms', '/produccion', '/distribucion'],
  LiderPlm:          ['/', '/plm'],
  LiderProduccion:   ['/', '/sag', '/produccion'],
  LiderBodega:       ['/', '/wms'],
  LiderDistribucion: ['/', '/distribucion'],
  Disenadora:        ['/', '/plm'],
  Vendedor:          ['/', '/plm/correria'],
  Operario:          ['/', '/wms'],
}

export function canAccess(role: string, path: string): boolean {
  const allowed = ROLE_NAV[role] ?? []
  return allowed.some(r => path === r || path.startsWith(r + '/'))
}

export function navItemsForRole(role: string) {
  const all = [
    { href: '/',             label: 'Inicio',       icon: '⌂' },
    { href: '/plm',          label: 'PLM',          icon: '◈' },
    { href: '/sag',          label: 'SAG',          icon: '◉' },
    { href: '/wms',          label: 'WMS',          icon: '◫' },
    { href: '/produccion',   label: 'Producción',   icon: '⚙' },
    { href: '/distribucion', label: 'Distribución', icon: '◎' },
  ]
  const allowed = ROLE_NAV[role] ?? ['/']
  return all.filter(item => allowed.some(r => item.href === r || r.startsWith(item.href + '/')))
}
