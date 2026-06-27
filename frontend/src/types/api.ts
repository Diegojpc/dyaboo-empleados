// ── PLM ───────────────────────────────────────────────────────────────────
export interface VariantDto {
  id: string
  sku: string
  sizeCode: string
  colorName: string
  colorHex: string
  costPrice: number
  stockQuantity: number
}

export interface ProductReferenceDto {
  id: string
  name: string
  referenceCode: string
  category: string
  description: string
  isActive: boolean
  variants: VariantDto[]
  createdAt: string
}

export type ProductCategory = 'Pantalon' | 'Camisa' | 'Chaqueta' | 'Vestido' | 'Accesorio'
export const CATEGORY_OPTIONS: { label: string; value: number }[] = [
  { label: 'Pantalón',   value: 1 },
  { label: 'Camisa',     value: 2 },
  { label: 'Chaqueta',   value: 3 },
  { label: 'Vestido',    value: 4 },
  { label: 'Accesorio',  value: 5 },
]

// ── SAG ───────────────────────────────────────────────────────────────────
export interface CostSummary {
  totalMaterialCost: number
  totalLaborCost: number
  totalOverheadCost: number
  grandTotal: number
  costPerUnit: number
}

export interface CostLineResult {
  variantId: string
  sku: string
  size: string
  color: string
  quantity: number
  materialCostPerUnit: number
  laborCostPerUnit: number
  overheadCostPerUnit: number
  totalCostPerUnit: number
  totalLineCost: number
}

export interface ProductionCostResult {
  orderId: string
  orderCode: string
  productReferenceName: string
  currency: string
  totalUnits: number
  overheadPercentage: number
  summary: CostSummary
  lines: CostLineResult[]
  calculatedAt: string
}

export interface VariantFinancialDetail {
  variantId: string
  sku: string
  size: string
  color: string
  stockQuantity: number
  unitCost: number
  totalValue: number
}

export interface FinancialInventoryLine {
  productReferenceId: string
  referenceCode: string
  productName: string
  totalUnits: number
  totalValue: number
  variants: VariantFinancialDetail[]
}

export interface FinancialInventoryResult {
  currency: string
  grandTotalValue: number
  totalSkus: number
  totalUnitsInStock: number
  lines: FinancialInventoryLine[]
}

// ── WMS ───────────────────────────────────────────────────────────────────
export interface AssignmentDetail {
  locationCode: string
  aisle: string
  shelf: number
  level: number
  sku: string
  size: string
  color: string
  quantityAssigned: number
  locationRemainingSpace: number
}

export interface AssignStockResult {
  productReferenceId: string
  productName: string
  totalUnitsAssigned: number
  locationsUsed: number
  assignments: AssignmentDetail[]
}

export interface LocationStatus {
  locationCode: string
  shelf: number
  level: number
  capacity: number
  currentStock: number
  availableSpace: number
  occupancyPercentage: number
  skusPresent: string[]
}

export interface AisleStatus {
  aisleCode: string
  locationCount: number
  totalCapacity: number
  currentStock: number
  occupancyPercentage: number
  locations: LocationStatus[]
}

export interface WarehouseStatusResult {
  totalLocations: number
  activeLocations: number
  occupiedLocations: number
  totalCapacity: number
  totalStockUnits: number
  occupancyPercentage: number
  aisles: AisleStatus[]
}
