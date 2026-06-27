import { apiGet, apiPost } from './client'
import { getToken } from '@/lib/auth/session'
import type { ProductReferenceDto, ProductImageDto } from '@/types/api'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8080'

export const getProductReferences = () =>
  apiGet<ProductReferenceDto[]>('/api/plm/productreferences')

export const createProductReference = (body: {
  name: string
  referenceCode: string
  category: number
  description: string
  variants: { sizeCode: string; colorName: string; colorHex: string; sku: string; costPrice: number }[]
}) => apiPost<ProductReferenceDto>('/api/plm/productreferences', body)

export async function uploadProductImage(referenceId: string, file: File): Promise<ProductImageDto> {
  const form = new FormData()
  form.append('file', file)
  const token = getToken()
  const res = await fetch(`${API_BASE}/api/plm/productreferences/${referenceId}/images`, {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: form,
  })
  if (!res.ok) {
    const data = await res.json().catch(() => ({}))
    throw new Error(data.error ?? `Error ${res.status}`)
  }
  return res.json()
}

export async function deleteProductImage(referenceId: string, imageId: string): Promise<void> {
  const token = getToken()
  const res = await fetch(`${API_BASE}/api/plm/productreferences/${referenceId}/images/${imageId}`, {
    method: 'DELETE',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
  if (!res.ok) throw new Error(`Error ${res.status}`)
}
