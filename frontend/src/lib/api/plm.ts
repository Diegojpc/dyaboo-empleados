import { apiGet, apiPost } from './client'
import type { ProductReferenceDto } from '@/types/api'

export const getProductReferences = () =>
  apiGet<ProductReferenceDto[]>('/api/plm/productreferences')

export const createProductReference = (body: {
  name: string
  referenceCode: string
  category: number
  description: string
  variants: { sizeCode: string; colorName: string; colorHex: string; sku: string; costPrice: number }[]
}) => apiPost<ProductReferenceDto>('/api/plm/productreferences', body)
