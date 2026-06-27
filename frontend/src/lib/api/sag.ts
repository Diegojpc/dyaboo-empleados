import { apiGet, apiPost } from './client'
import type { FinancialInventoryResult, ProductionCostResult } from '@/types/api'

export const getFinancialInventory = () =>
  apiGet<FinancialInventoryResult>('/api/sag/financialinventory')

export const calculateProductionCost = (body: {
  productReferenceId: string
  overheadPercentage: number
  items: { variantId: string; quantity: number; laborCostPerUnit: number }[]
}) => apiPost<ProductionCostResult>('/api/sag/productioncosts', body)
