import { apiGet, apiPost } from './client'
import type { AssignStockResult, WarehouseStatusResult } from '@/types/api'

export const getWarehouseStatus = () =>
  apiGet<WarehouseStatusResult>('/api/wms/warehousestatus')

export const assignStock = (body: {
  productReferenceId: string
  items: { variantId: string; quantity: number }[]
}) => apiPost<AssignStockResult>('/api/wms/stockassignments', body)
