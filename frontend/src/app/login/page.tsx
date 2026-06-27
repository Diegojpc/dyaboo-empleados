'use client'
import { useState } from 'react'
import Image from 'next/image'
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
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-sm">
        {/* Logo + título */}
        <div className="text-center mb-8">
          <div className="flex justify-center mb-3">
            <Image
              src="/dyaboo-logo.png"
              alt="Dyaboo"
              width={80}
              height={80}
              className="object-contain"
            />
          </div>
          <h1 className="text-white text-xl font-bold">Dyaboo Empleados</h1>
          <p className="text-slate-400 text-sm mt-1">Acceso al sistema interno</p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="bg-slate-800 rounded-2xl p-6 shadow-xl space-y-4 border border-slate-700"
        >
          {error && (
            <div className="rounded-lg bg-red-900/50 border border-red-700 px-4 py-3 text-sm text-red-300">
              {error}
            </div>
          )}

          <div>
            <label className="block text-xs font-medium text-slate-400 mb-1.5">
              Correo electrónico
            </label>
            <input
              type="email"
              required
              autoFocus
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="ceo@dyaboo.com"
              className="w-full rounded-lg bg-slate-700 border border-slate-600 text-white
                         placeholder-slate-500 px-3 py-2.5 text-sm
                         focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-400 mb-1.5">
              Contraseña
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              className="w-full rounded-lg bg-slate-700 border border-slate-600 text-white
                         placeholder-slate-500 px-3 py-2.5 text-sm
                         focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-lg bg-indigo-600 text-white py-2.5 text-sm font-medium
                       hover:bg-indigo-700 disabled:opacity-50 transition-colors mt-2"
          >
            {loading ? 'Iniciando sesión...' : 'Ingresar'}
          </button>
        </form>

        <p className="text-center text-xs text-slate-600 mt-4">
          Solo para uso interno · MVP v0.1
        </p>
      </div>
    </div>
  )
}
