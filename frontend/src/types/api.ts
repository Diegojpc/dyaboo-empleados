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

export interface ProductImageDto {
  id: string
  fileName: string
  originalName: string
  url: string
  sortOrder: number
}

export interface ProductReferenceDto {
  id: string
  name: string
  referenceCode: string
  category: string
  description: string
  isActive: boolean
  variants: VariantDto[]
  images: ProductImageDto[]
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
  colorName: string
  colorHex: string
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

// ── Producción ────────────────────────────────────────────────────────────
export interface ConfeccionistaDto {
  id: string
  name: string
  contactName: string
  phone: string
  city: string
  isActive: boolean
}

export interface CuttingOrderItemDto {
  id: string
  sku: string
  size: string
  color: string
  plannedQuantity: number
  cutQuantity: number
}

export interface CuttingOrderDto {
  id: string
  orderCode: string
  productReferenceName: string
  status: 'InProgress' | 'Completed'
  notes: string | null
  totalPlannedUnits: number
  totalCutUnits: number
  hasSewingOrder: boolean
  items: CuttingOrderItemDto[]
  createdAt: string
  completedAt: string | null
}

export interface SewingOrderItemDto {
  id: string
  sku: string
  size: string
  color: string
  quantitySent: number
  quantityApproved: number
  quantityRejected: number
}

export interface SewingOrderDto {
  id: string
  orderCode: string
  cuttingOrderCode: string
  confeccionistaName: string
  status: 'Assigned' | 'Received'
  totalSent: number
  totalApproved: number
  totalRejected: number
  items: SewingOrderItemDto[]
  createdAt: string
  receivedAt: string | null
}

// ── Distribución ──────────────────────────────────────────────────────────
export interface CustomerDto {
  id: string
  name: string
  type: 'TiendaPropia' | 'MayoristaExterno'
  contactName: string
  phone: string
  city: string
  isActive: boolean
}

export interface SalesOrderItemDto {
  id: string
  sku: string
  size: string
  color: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export type SalesOrderStatus = 'Draft' | 'Confirmed' | 'Dispatched' | 'Delivered' | 'Cancelled'

export interface SalesOrderDto {
  id: string
  orderCode: string
  customerName: string
  customerType: string
  status: SalesOrderStatus
  notes: string | null
  totalUnits: number
  total: number
  items: SalesOrderItemDto[]
  createdAt: string
  confirmedAt: string | null
  dispatchedAt: string | null
  deliveredAt: string | null
}

export interface PickingLine {
  locationCode: string
  sku: string
  quantity: number
}

export interface DispatchResult {
  orderId: string
  orderCode: string
  totalUnitsDispatched: number
  pickingLines: PickingLine[]
}

// ── RRHH ──────────────────────────────────────────────────────────────────
export type CompanyArea =
  | 'Diseno' | 'Mercadeo' | 'Comercial' | 'Tiendas'
  | 'Corte' | 'Produccion' | 'Logistica'
  | 'Financiera' | 'Proyectos'

export type OvertimeType =
  | 'ExtraDiurna' | 'ExtraNocturna' | 'RecargoNocturno'
  | 'DominicalFestivo' | 'ExtraDiurnaDominical' | 'ExtraNocturnaDominical'

export interface EmpleadoDto {
  id: string
  fullName: string
  documentNumber: string
  jobTitle: string
  area: CompanyArea
  direction: string
  hireDate: string
  monthlySalary: number
  weeklyHours: number
  hourlyRate: number
  isActive: boolean
}

export interface NovedadHorasDto {
  id: string
  employeeId: string
  employeeName: string
  date: string
  type: OvertimeType
  hours: number
  hourlyRateSnapshot: number
  surchargePercent: number
  amount: number
  notes: string | null
}

export interface PeriodoVacacionesDto {
  id: string
  startDate: string
  endDate: string
  businessDays: number
  notes: string | null
}

export interface SaldoVacacionesDto {
  employeeId: string
  fullName: string
  jobTitle: string
  hireDate: string
  accruedDays: number
  takenDays: number
  balanceDays: number
  periods: PeriodoVacacionesDto[]
}

export interface FestivoDto {
  date: string
  name: string
}

export interface FilaResumenMensualDto {
  employeeId: string
  fullName: string
  documentNumber: string
  jobTitle: string
  monthlySalary: number
  horasExtraDiurna: number
  horasExtraNocturna: number
  horasRecargoNocturno: number
  horasDominicalFestivo: number
  horasExtraDiurnaDominical: number
  horasExtraNocturnaDominical: number
  totalHoras: number
  totalRecargos: number
  diasVacaciones: number
}
