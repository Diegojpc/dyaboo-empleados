import { apiGet, apiPost } from './client'
import type {
  EmpleadoDto, NovedadHorasDto, SaldoVacacionesDto, PeriodoVacacionesDto,
  FestivoDto, FilaResumenMensualDto,
} from '@/types/api'

export const getEmpleados = () =>
  apiGet<EmpleadoDto[]>('/api/rrhh/employees')

export const crearEmpleado = (body: {
  fullName: string
  documentNumber: string
  jobTitle: string
  area: number // índice del enum CompanyArea (1-9)
  hireDate: string
  monthlySalary: number
  weeklyHours: number
}) => apiPost<EmpleadoDto>('/api/rrhh/employees', body)

export const desactivarEmpleado = (id: string) =>
  apiPost<EmpleadoDto>(`/api/rrhh/employees/${id}/deactivate`, {})

export const getNovedades = (year?: number, month?: number) => {
  const params = new URLSearchParams()
  if (year) params.set('year', String(year))
  if (month) params.set('month', String(month))
  const qs = params.toString()
  return apiGet<NovedadHorasDto[]>(`/api/rrhh/overtime${qs ? `?${qs}` : ''}`)
}

export const registrarNovedad = (body: {
  employeeId: string
  date: string
  type: number // índice del enum OvertimeType (1-6)
  hours: number
  notes: string | null
}) => apiPost<NovedadHorasDto>('/api/rrhh/overtime', body)

export const getVacaciones = () =>
  apiGet<SaldoVacacionesDto[]>('/api/rrhh/vacations')

export const registrarVacaciones = (body: {
  employeeId: string
  startDate: string
  endDate: string
  notes: string | null
}) => apiPost<PeriodoVacacionesDto>('/api/rrhh/vacations', body)

export const getFestivos = (year?: number) =>
  apiGet<FestivoDto[]>(`/api/rrhh/holidays${year ? `?year=${year}` : ''}`)

export const getResumenMensual = (year: number, month: number) =>
  apiGet<FilaResumenMensualDto[]>(`/api/rrhh/summary?year=${year}&month=${month}`)
