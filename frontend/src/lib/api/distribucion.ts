import { apiGet, apiPost } from './client'
import type { CustomerDto, SalesOrderDto, DispatchResult } from '@/types/api'

export const getCustomers = () =>
  apiGet<CustomerDto[]>('/api/distribucion/customers')

export const createCustomer = (body: {
  name: string
  type: number // 1 = TiendaPropia, 2 = MayoristaExterno
  contactName: string
  phone: string
  city: string
}) => apiPost<CustomerDto>('/api/distribucion/customers', body)

export const getSalesOrders = () =>
  apiGet<SalesOrderDto[]>('/api/distribucion/salesorders')

export const createSalesOrder = (body: {
  customerId: string
  items: { variantId: string; quantity: number; unitPrice: number }[]
  notes: string | null
}) => apiPost<SalesOrderDto>('/api/distribucion/salesorders', body)

export const confirmSalesOrder = (id: string) =>
  apiPost<SalesOrderDto>(`/api/distribucion/salesorders/${id}/confirm`, {})

export const dispatchSalesOrder = (id: string) =>
  apiPost<DispatchResult>(`/api/distribucion/salesorders/${id}/dispatch`, {})

export const deliverSalesOrder = (id: string) =>
  apiPost<SalesOrderDto>(`/api/distribucion/salesorders/${id}/deliver`, {})

export const cancelSalesOrder = (id: string) =>
  apiPost<SalesOrderDto>(`/api/distribucion/salesorders/${id}/cancel`, {})
