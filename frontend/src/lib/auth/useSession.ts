'use client'
import { useEffect, useState } from 'react'
import { getSessionUser } from './session'
import type { SessionUser } from './session'

export function useSession() {
  const [user, setUser] = useState<Omit<SessionUser, 'token'> | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setUser(getSessionUser())
    setLoading(false)
  }, [])

  return { user, loading }
}
