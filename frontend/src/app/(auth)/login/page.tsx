'use client'
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { apiLogin } from '@/lib/api/client'
import { saveSession } from '@/lib/auth/session'

export default function LoginPage() {
  const router = useRouter()
  const [email, setEmail]       = useState('')
  const [password, setPassword] = useState('')
  const [error, setError]       = useState('')
  const [loading, setLoading]   = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await apiLogin(email, password)
      saveSession(data)
      router.replace('/')
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4" style={{ backgroundColor: '#000' }}>
      <div className="w-full max-w-sm">

        {/* Logo */}
        <div className="text-center mb-8">
          <div className="flex justify-center mb-6">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src="/dyaboo-logo.png"
              alt="Dyaboo"
              style={{ width: 160, height: 'auto', filter: 'brightness(0) invert(1)' }}
            />
          </div>
          <p className="text-sm" style={{ color: '#7c7975' }}>
            Acceso al sistema interno
          </p>
        </div>

        {/* Formulario */}
        <div className="rounded-2xl p-6 space-y-4" style={{
          backgroundColor: '#111',
          border: '1px solid #2a2a2a',
        }}>
          {error && (
            <div className="rounded-lg px-4 py-3 text-sm" style={{
              backgroundColor: 'rgba(139,0,0,0.2)',
              border: '1px solid rgba(139,0,0,0.5)',
              color: '#fca5a5',
            }}>
              {error}
            </div>
          )}

          <div>
            <label className="block text-xs font-medium mb-1.5" style={{ color: '#7c7975' }}>
              Correo electrónico
            </label>
            <input
              type="email"
              required
              autoFocus
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="ceo@dyaboo.com"
              className="w-full rounded-lg px-3 py-2.5 text-sm focus:outline-none transition-colors"
              style={{
                backgroundColor: '#1a1a1a',
                border: '1px solid #2a2a2a',
                color: '#F0EFEB',
              }}
              onFocus={e => e.target.style.borderColor = '#108474'}
              onBlur={e => e.target.style.borderColor = '#2a2a2a'}
            />
          </div>

          <div>
            <label className="block text-xs font-medium mb-1.5" style={{ color: '#7c7975' }}>
              Contraseña
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              className="w-full rounded-lg px-3 py-2.5 text-sm focus:outline-none transition-colors"
              style={{
                backgroundColor: '#1a1a1a',
                border: '1px solid #2a2a2a',
                color: '#F0EFEB',
              }}
              onFocus={e => e.target.style.borderColor = '#108474'}
              onBlur={e => e.target.style.borderColor = '#2a2a2a'}
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            onClick={handleSubmit}
            className="w-full rounded-lg py-2.5 text-sm font-medium transition-colors mt-2 disabled:opacity-50"
            style={{ backgroundColor: '#108474', color: '#fff' }}
            onMouseEnter={e => !loading && ((e.target as HTMLElement).style.backgroundColor = '#0a6359')}
            onMouseLeave={e => !loading && ((e.target as HTMLElement).style.backgroundColor = '#108474')}
          >
            {loading ? 'Iniciando sesión...' : 'Ingresar'}
          </button>
        </div>

        <p className="text-center text-xs mt-4" style={{ color: '#3a3a3a' }}>
          Solo para uso interno · MVP v0.1
        </p>
      </div>
    </div>
  )
}
