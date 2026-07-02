import { apiGet, apiPost } from './client'
import type { ConfeccionistaDto, CuttingOrderDto, SewingOrderDto } from '@/types/api'

export const getConfeccionistas = () =>
  apiGet<ConfeccionistaDto[]>('/api/produccion/confeccionistas')

export const createConfeccionista = (body: {
  name: string
  contactName: string
  phone: string
  city: string
}) => apiPost<ConfeccionistaDto>('/api/produccion/confeccionistas', body)

export const getCuttingOrders = () =>
  apiGet<CuttingOrderDto[]>('/api/produccion/cuttingorders')

export const createCuttingOrder = (body: {
  productReferenceId: string
  items: { variantId: string; quantity: number }[]
  notes: string | null
}) => apiPost<CuttingOrderDto>('/api/produccion/cuttingorders', body)

export const completeCuttingOrder = (id: string, items: { itemId: string; cutQuantity: number }[]) =>
  apiPost<CuttingOrderDto>(`/api/produccion/cuttingorders/${id}/complete`, { items })

export const getSewingOrders = () =>
  apiGet<SewingOrderDto[]>('/api/produccion/sewingorders')

export const createSewingOrder = (body: {
  cuttingOrderId: string
  confeccionistaId: string
}) => apiPost<SewingOrderDto>('/api/produccion/sewingorders', body)

export const receiveSewingOrder = (id: string, items: { itemId: string; approved: number; rejected: number }[]) =>
  apiPost<SewingOrderDto>(`/api/produccion/sewingorders/${id}/receive`, { items })
