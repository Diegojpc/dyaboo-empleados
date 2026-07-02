'use client'
import { useEffect, useState } from 'react'
import {
  getEmpleados, crearEmpleado, desactivarEmpleado,
  getNovedades, registrarNovedad,
  getVacaciones, registrarVacaciones,
  getFestivos, getResumenMensual,
} from '@/lib/api/rrhh'
import type {
  EmpleadoDto, NovedadHorasDto, SaldoVacacionesDto, FestivoDto, FilaResumenMensualDto,
} from '@/types/api'
import { num, cop } from '@/lib/utils/format'

// Opciones de área con su dirección (mismo mapa del organigrama en el backend)
const AREAS: { value: number; label: string; direccion: string }[] = [
  { value: 1, label: 'Diseño',     direccion: 'Comercial' },
  { value: 2, label: 'Mercadeo',   direccion: 'Comercial' },
  { value: 3, label: 'Comercial',  direccion: 'Comercial' },
  { value: 4, label: 'Tiendas',    direccion: 'Comercial' },
  { value: 5, label: 'Corte',      direccion: 'Operaciones' },
  { value: 6, label: 'Producción', direccion: 'Operaciones' },
  { value: 7, label: 'Logística',  direccion: 'Operaciones' },
  { value: 8, label: 'Financiera', direccion: 'Administrativa' },
  { value: 9, label: 'Proyectos',  direccion: 'Administrativa' },
]

const AREA_LABELS: Record<string, string> = {
  Diseno: 'Diseño', Mercadeo: 'Mercadeo', Comercial: 'Comercial', Tiendas: 'Tiendas',
  Corte: 'Corte', Produccion: 'Producción', Logistica: 'Logística',
  Financiera: 'Financiera', Proyectos: 'Proyectos',
}

// Recargo dominical/festivo progresivo (Ley 2466/2025) — espejo del backend
function pctDominical(fecha: string): number {
  if (fecha >= '2027-07-01') return 1.0
  if (fecha >= '2026-07-01') return 0.9
  if (fecha >= '2025-07-01') return 0.8
  return 0.75
}

const TIPOS_NOVEDAD: { value: number; label: string; pct: (fecha: string) => number; esExtra: boolean }[] = [
  { value: 1, label: 'Hora extra diurna',            pct: () => 0.25,                     esExtra: true },
  { value: 2, label: 'Hora extra nocturna',          pct: () => 0.75,                     esExtra: true },
  { value: 3, label: 'Recargo nocturno',             pct: () => 0.35,                     esExtra: false },
  { value: 4, label: 'Dominical / festivo',          pct: pctDominical,                   esExtra: false },
  { value: 5, label: 'Extra diurna dominical',       pct: f => 0.25 + pctDominical(f),    esExtra: true },
  { value: 6, label: 'Extra nocturna dominical',     pct: f => 0.75 + pctDominical(f),    esExtra: true },
]

const TIPO_LABELS: Record<string, string> = {
  ExtraDiurna: 'Extra diurna', ExtraNocturna: 'Extra nocturna', RecargoNocturno: 'Rec. nocturno',
  DominicalFestivo: 'Dominical/festivo', ExtraDiurnaDominical: 'Extra diurna dom.',
  ExtraNocturnaDominical: 'Extra nocturna dom.',
}

function ErrorBox({ message }: { message: string }) {
  if (!message) return null
  return (
    <div className="rounded-lg px-4 py-3 text-sm"
      style={{ backgroundColor: 'rgba(139,0,0,0.1)', border: '1px solid rgba(139,0,0,0.3)', color: '#dc2626' }}>
      {message}
    </div>
  )
}

function OkBox({ message, onClose }: { message: string; onClose: () => void }) {
  if (!message) return null
  return (
    <div className="rounded-lg px-4 py-3 text-sm flex items-center justify-between"
      style={{ backgroundColor: 'rgba(22,163,74,0.1)', border: '1px solid rgba(22,163,74,0.3)', color: '#16a34a' }}>
      <span>{message}</span>
      <button onClick={onClose} className="text-xs opacity-70 hover:opacity-100">✕</button>
    </div>
  )
}

export default function RrhhPage() {
  const hoy = new Date().toISOString().slice(0, 10)

  const [empleados, setEmpleados] = useState<EmpleadoDto[]>([])
  const [novedades, setNovedades] = useState<NovedadHorasDto[]>([])
  const [saldos, setSaldos]       = useState<SaldoVacacionesDto[]>([])
  const [festivos, setFestivos]   = useState<FestivoDto[]>([])
  const [resumen, setResumen]     = useState<FilaResumenMensualDto[]>([])

  // Form empleado
  const [showEmpForm, setShowEmpForm] = useState(false)
  const [empForm, setEmpForm] = useState({
    fullName: '', documentNumber: '', jobTitle: '', area: 5,
    hireDate: hoy, monthlySalary: '1623500', weeklyHours: '44',
  })
  const [empError, setEmpError] = useState('')

  // Form novedad
  const [novForm, setNovForm] = useState({ employeeId: '', date: hoy, type: 1, hours: '1', notes: '' })
  const [novError, setNovError] = useState('')
  const [novOk, setNovOk] = useState('')

  // Form vacaciones
  const [vacForm, setVacForm] = useState({ employeeId: '', startDate: hoy, endDate: hoy, notes: '' })
  const [vacError, setVacError] = useState('')
  const [vacOk, setVacOk] = useState('')

  // Resumen
  const [periodo, setPeriodo] = useState(() => hoy.slice(0, 7)) // YYYY-MM
  const [resumenError, setResumenError] = useState('')

  const [anio, mes] = periodo.split('-').map(Number)

  const loadEmpleados = () => getEmpleados().then(e => {
    setEmpleados(e)
    setNovForm(f => ({ ...f, employeeId: f.employeeId || e.find(x => x.isActive)?.id || '' }))
    setVacForm(f => ({ ...f, employeeId: f.employeeId || e.find(x => x.isActive)?.id || '' }))
  }).catch(console.error)
  const loadNovedades = () => getNovedades().then(setNovedades).catch(console.error)
  const loadSaldos    = () => getVacaciones().then(setSaldos).catch(console.error)
  const loadResumen   = (y: number, m: number) =>
    getResumenMensual(y, m).then(setResumen).catch(e => setResumenError((e as Error).message))

  useEffect(() => {
    loadEmpleados(); loadNovedades(); loadSaldos()
    getFestivos(new Date().getFullYear()).then(setFestivos).catch(console.error)
    loadResumen(anio, mes)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const activos = empleados.filter(e => e.isActive)
  const tipoSel = TIPOS_NOVEDAD.find(t => t.value === novForm.type)!
  const empleadoSel = empleados.find(e => e.id === novForm.employeeId)
  const pctSel = tipoSel.pct(novForm.date)
  const valorEstimado = empleadoSel
    ? parseFloat(novForm.hours || '0') * empleadoSel.hourlyRate * (tipoSel.esExtra ? 1 + pctSel : pctSel)
    : 0

  const handleCrearEmpleado = async (e: React.FormEvent) => {
    e.preventDefault()
    setEmpError('')
    try {
      await crearEmpleado({
        fullName: empForm.fullName,
        documentNumber: empForm.documentNumber,
        jobTitle: empForm.jobTitle,
        area: empForm.area,
        hireDate: empForm.hireDate,
        monthlySalary: parseFloat(empForm.monthlySalary),
        weeklyHours: parseInt(empForm.weeklyHours),
      })
      setEmpForm(f => ({ ...f, fullName: '', documentNumber: '', jobTitle: '' }))
      setShowEmpForm(false)
      loadEmpleados(); loadSaldos()
    } catch (err) { setEmpError((err as Error).message) }
  }

  const handleDesactivar = async (id: string) => {
    try { await desactivarEmpleado(id); loadEmpleados(); loadSaldos() }
    catch (err) { setEmpError((err as Error).message) }
  }

  const handleNovedad = async (e: React.FormEvent) => {
    e.preventDefault()
    setNovError(''); setNovOk('')
    try {
      const res = await registrarNovedad({
        employeeId: novForm.employeeId,
        date: novForm.date,
        type: novForm.type,
        hours: parseFloat(novForm.hours),
        notes: novForm.notes.trim() || null,
      })
      setNovOk(`Registrada: ${res.hours} h al ${(res.surchargePercent * 100).toFixed(0)}% → ${cop(res.amount)}`)
      loadNovedades(); loadResumen(anio, mes)
    } catch (err) { setNovError((err as Error).message) }
  }

  const handleVacaciones = async (e: React.FormEvent) => {
    e.preventDefault()
    setVacError(''); setVacOk('')
    try {
      const res = await registrarVacaciones({
        employeeId: vacForm.employeeId,
        startDate: vacForm.startDate,
        endDate: vacForm.endDate,
        notes: vacForm.notes.trim() || null,
      })
      setVacOk(`Periodo registrado: ${res.businessDays} días hábiles (${res.startDate} → ${res.endDate})`)
      loadSaldos(); loadResumen(anio, mes)
    } catch (err) { setVacError((err as Error).message) }
  }

  const handlePeriodo = (value: string) => {
    setPeriodo(value); setResumenError('')
    const [y, m] = value.split('-').map(Number)
    if (y && m) loadResumen(y, m)
  }

  const exportarCsv = () => {
    const headers = [
      'Cédula', 'Nombre', 'Cargo', 'Salario',
      'H. extra diurna', 'H. extra nocturna', 'H. recargo nocturno', 'H. dominical/festivo',
      'H. extra diurna dom.', 'H. extra nocturna dom.', 'Total horas', 'Total recargos', 'Días vacaciones',
    ]
    const rows = resumen.map(r => [
      r.documentNumber, r.fullName, r.jobTitle, r.monthlySalary,
      r.horasExtraDiurna, r.horasExtraNocturna, r.horasRecargoNocturno, r.horasDominicalFestivo,
      r.horasExtraDiurnaDominical, r.horasExtraNocturnaDominical, r.totalHoras, r.totalRecargos, r.diasVacaciones,
    ])
    const csv = [headers, ...rows]
      .map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(';'))
      .join('\r\n')
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8' })
    const url  = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `novedades-${periodo}.csv`; a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>RRHH — Gestión Humana</h1>
        <p>Empleados, horas extra y recargos (RIT / Ley 2466), vacaciones y novedades para nómina</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Columna izquierda */}
        <div className="space-y-4">
          {/* Empleados */}
          <div className="card overflow-hidden">
            <div className="card-header flex items-center justify-between">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Empleados</h2>
              <button onClick={() => setShowEmpForm(s => !s)} className="text-xs accent hover:underline">
                {showEmpForm ? 'Cancelar' : '+ Nuevo empleado'}
              </button>
            </div>

            {showEmpForm && (
              <form onSubmit={handleCrearEmpleado} className="p-4 space-y-2" style={{ borderBottom: '1px solid var(--border)' }}>
                <ErrorBox message={empError} />
                <div className="grid grid-cols-2 gap-2">
                  <input className="input text-sm" placeholder="Nombre completo" required
                    value={empForm.fullName} onChange={e => setEmpForm(f => ({ ...f, fullName: e.target.value }))} />
                  <input className="input text-sm" placeholder="Cédula" required
                    value={empForm.documentNumber} onChange={e => setEmpForm(f => ({ ...f, documentNumber: e.target.value }))} />
                  <input className="input text-sm" placeholder="Cargo (según organigrama)" required
                    value={empForm.jobTitle} onChange={e => setEmpForm(f => ({ ...f, jobTitle: e.target.value }))} />
                  <select className="input text-sm" value={empForm.area}
                    onChange={e => setEmpForm(f => ({ ...f, area: parseInt(e.target.value) }))}>
                    {AREAS.map(a => <option key={a.value} value={a.value}>{a.label} — {a.direccion}</option>)}
                  </select>
                  <div>
                    <label className="block text-[10px] muted mb-0.5">Fecha de ingreso</label>
                    <input type="date" className="input text-sm" required
                      value={empForm.hireDate} onChange={e => setEmpForm(f => ({ ...f, hireDate: e.target.value }))} />
                  </div>
                  <div>
                    <label className="block text-[10px] muted mb-0.5">Salario mensual</label>
                    <input type="number" min="1" className="input text-sm" required
                      value={empForm.monthlySalary} onChange={e => setEmpForm(f => ({ ...f, monthlySalary: e.target.value }))} />
                  </div>
                  <div>
                    <label className="block text-[10px] muted mb-0.5">Jornada semanal (horas)</label>
                    <input type="number" min="1" max="60" className="input text-sm" required
                      value={empForm.weeklyHours} onChange={e => setEmpForm(f => ({ ...f, weeklyHours: e.target.value }))} />
                  </div>
                </div>
                <button type="submit" className="btn-primary w-full">Guardar empleado</button>
              </form>
            )}

            {!showEmpForm && empError && <div className="p-4"><ErrorBox message={empError} /></div>}

            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead><tr className="table-head">
                  {['Empleado', 'Cargo', 'Área', 'Salario', 'Hora', ''].map((h, i) => <th key={i}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {empleados.map(e => (
                    <tr key={e.id} className="table-row" style={{ opacity: e.isActive ? 1 : 0.45 }}>
                      <td className="px-3 py-2">
                        <div className="font-medium">{e.fullName}</div>
                        <div className="muted text-[10px]">CC {e.documentNumber}</div>
                      </td>
                      <td className="px-3 py-2 muted">{e.jobTitle}</td>
                      <td className="px-3 py-2">
                        <span className="badge text-[10px]">{AREA_LABELS[e.area] ?? e.area}</span>
                      </td>
                      <td className="px-3 py-2">{cop(e.monthlySalary)}</td>
                      <td className="px-3 py-2 muted">{cop(e.hourlyRate)}</td>
                      <td className="px-3 py-2">
                        {e.isActive ? (
                          <button onClick={() => handleDesactivar(e.id)}
                            className="text-[10px] hover:underline" style={{ color: '#dc2626' }}>
                            Retirar
                          </button>
                        ) : <span className="text-[10px] subtle">Inactivo</span>}
                      </td>
                    </tr>
                  ))}
                  {empleados.length === 0 && (
                    <tr><td colSpan={6} className="px-4 py-6 text-center subtle">Sin empleados</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Vacaciones */}
          <div className="card overflow-hidden">
            <div className="card-header">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Vacaciones</h2>
              <p className="text-xs subtle mt-0.5">15 días hábiles/año · sábado cuenta · excluye domingos y festivos</p>
            </div>

            <form onSubmit={handleVacaciones} className="p-4 space-y-2" style={{ borderBottom: '1px solid var(--border)' }}>
              <ErrorBox message={vacError} />
              <OkBox message={vacOk} onClose={() => setVacOk('')} />
              <div className="grid grid-cols-3 gap-2">
                <select className="input text-sm col-span-3" value={vacForm.employeeId}
                  onChange={e => setVacForm(f => ({ ...f, employeeId: e.target.value }))}>
                  {activos.map(e => <option key={e.id} value={e.id}>{e.fullName}</option>)}
                </select>
                <div>
                  <label className="block text-[10px] muted mb-0.5">Desde</label>
                  <input type="date" className="input text-sm" required
                    value={vacForm.startDate} onChange={e => setVacForm(f => ({ ...f, startDate: e.target.value }))} />
                </div>
                <div>
                  <label className="block text-[10px] muted mb-0.5">Hasta</label>
                  <input type="date" className="input text-sm" required
                    value={vacForm.endDate} onChange={e => setVacForm(f => ({ ...f, endDate: e.target.value }))} />
                </div>
                <div className="flex items-end">
                  <button type="submit" className="btn-primary w-full">Registrar</button>
                </div>
              </div>
            </form>

            <table className="w-full text-xs">
              <thead><tr className="table-head">
                {['Empleado', 'Causadas', 'Disfrutadas', 'Saldo'].map(h => <th key={h}>{h}</th>)}
              </tr></thead>
              <tbody>
                {saldos.map(s => (
                  <tr key={s.employeeId} className="table-row">
                    <td className="px-3 py-2 font-medium">{s.fullName}</td>
                    <td className="px-3 py-2">{s.accruedDays.toFixed(2)}</td>
                    <td className="px-3 py-2">{s.takenDays}</td>
                    <td className="px-3 py-2 font-semibold"
                      style={{ color: s.balanceDays < 0 ? '#dc2626' : 'var(--text)' }}>
                      {s.balanceDays.toFixed(2)}
                    </td>
                  </tr>
                ))}
                {saldos.length === 0 && (
                  <tr><td colSpan={4} className="px-4 py-6 text-center subtle">Sin empleados activos</td></tr>
                )}
              </tbody>
            </table>

            {festivos.length > 0 && (
              <div className="p-4" style={{ borderTop: '1px solid var(--border)' }}>
                <p className="text-[10px] muted mb-1.5">Festivos {new Date().getFullYear()}</p>
                <div className="flex flex-wrap gap-1">
                  {festivos.map(f => (
                    <span key={f.date} className="badge text-[10px]" title={f.name}>
                      {f.date.slice(5)}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Columna derecha */}
        <div className="space-y-4">
          {/* Novedades de horas */}
          <div className="card overflow-hidden">
            <div className="card-header">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Novedades de horas</h2>
              <p className="text-xs subtle mt-0.5">Extra diurna 25% · extra nocturna 75% · rec. nocturno 35% · dominical según fecha</p>
            </div>

            <form onSubmit={handleNovedad} className="p-4 space-y-2" style={{ borderBottom: '1px solid var(--border)' }}>
              <ErrorBox message={novError} />
              <OkBox message={novOk} onClose={() => setNovOk('')} />
              <div className="grid grid-cols-2 gap-2">
                <select className="input text-sm" value={novForm.employeeId}
                  onChange={e => setNovForm(f => ({ ...f, employeeId: e.target.value }))}>
                  {activos.map(e => <option key={e.id} value={e.id}>{e.fullName}</option>)}
                </select>
                <input type="date" className="input text-sm" required
                  value={novForm.date} onChange={e => setNovForm(f => ({ ...f, date: e.target.value }))} />
                <select className="input text-sm" value={novForm.type}
                  onChange={e => setNovForm(f => ({ ...f, type: parseInt(e.target.value) }))}>
                  {TIPOS_NOVEDAD.map(t => (
                    <option key={t.value} value={t.value}>
                      {t.label} ({(t.pct(novForm.date) * 100).toFixed(0)}%)
                    </option>
                  ))}
                </select>
                <input type="number" min="0.5" step="0.5" className="input text-sm" required placeholder="Horas"
                  value={novForm.hours} onChange={e => setNovForm(f => ({ ...f, hours: e.target.value }))} />
                <input className="input text-sm col-span-2" placeholder="Notas (opcional)" maxLength={500}
                  value={novForm.notes} onChange={e => setNovForm(f => ({ ...f, notes: e.target.value }))} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs muted">
                  Valor estimado {tipoSel.esExtra ? '(hora + recargo)' : '(solo recargo)'}
                </span>
                <span className="font-semibold accent">{cop(valorEstimado || 0)}</span>
              </div>
              <button type="submit" disabled={!novForm.employeeId} className="btn-primary w-full">
                Registrar novedad
              </button>
            </form>

            <div className="overflow-x-auto" style={{ maxHeight: '18rem', overflowY: 'auto' }}>
              <table className="w-full text-xs">
                <thead><tr className="table-head">
                  {['Fecha', 'Empleado', 'Tipo', 'Horas', '%', 'Valor'].map(h => <th key={h}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {novedades.map(n => (
                    <tr key={n.id} className="table-row">
                      <td className="px-3 py-1.5 font-mono">{n.date}</td>
                      <td className="px-3 py-1.5">{n.employeeName}</td>
                      <td className="px-3 py-1.5"><span className="badge text-[10px]">{TIPO_LABELS[n.type] ?? n.type}</span></td>
                      <td className="px-3 py-1.5">{n.hours}</td>
                      <td className="px-3 py-1.5 muted">{(n.surchargePercent * 100).toFixed(0)}%</td>
                      <td className="px-3 py-1.5 font-medium">{cop(n.amount)}</td>
                    </tr>
                  ))}
                  {novedades.length === 0 && (
                    <tr><td colSpan={6} className="px-4 py-6 text-center subtle">Sin novedades registradas</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Resumen mensual */}
          <div className="card overflow-hidden">
            <div className="card-header flex items-center justify-between">
              <div>
                <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Resumen mensual</h2>
                <p className="text-xs subtle mt-0.5">Novedades para llevar al software contable</p>
              </div>
              <div className="flex items-center gap-2">
                <input type="month" className="input text-sm w-40" value={periodo}
                  onChange={e => handlePeriodo(e.target.value)} />
                <button onClick={exportarCsv} disabled={resumen.length === 0} className="btn-primary text-xs shrink-0">
                  Exportar CSV
                </button>
              </div>
            </div>

            {resumenError && <div className="p-4"><ErrorBox message={resumenError} /></div>}

            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead><tr className="table-head">
                  {['Empleado', 'Total horas', 'Total recargos', 'Días vacaciones'].map(h => <th key={h}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {resumen.map(r => (
                    <tr key={r.employeeId} className="table-row">
                      <td className="px-3 py-2">
                        <div className="font-medium">{r.fullName}</div>
                        <div className="muted text-[10px]">{r.jobTitle}</div>
                      </td>
                      <td className="px-3 py-2">{num(r.totalHoras)}</td>
                      <td className="px-3 py-2 font-medium">{cop(r.totalRecargos)}</td>
                      <td className="px-3 py-2">{r.diasVacaciones}</td>
                    </tr>
                  ))}
                  {resumen.length === 0 && (
                    <tr><td colSpan={4} className="px-4 py-6 text-center subtle">Sin novedades en el periodo</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
