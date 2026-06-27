'use client'
import { useEffect, useState } from 'react'
import { assignStock, getWarehouseStatus } from '@/lib/api/wms'
import { getProductReferences } from '@/lib/api/plm'
import type { ProductReferenceDto, AssignStockResult, WarehouseStatusResult, AisleStatus } from '@/types/api'
import { num, pct } from '@/lib/utils/format'

function OccupancyBar({ value }: { value: number }) {
  const color = value >= 100 ? 'bg-red-500' : value >= 75 ? 'bg-amber-500' : value > 0 ? 'bg-emerald-500' : 'bg-slate-200'
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 bg-slate-100 rounded-full h-2 overflow-hidden">
        <div className={`h-full rounded-full transition-all ${color}`} style={{ width: `${Math.min(value, 100)}%` }} />
      </div>
      <span className="text-xs text-slate-500 w-12 text-right">{pct(value)}</span>
    </div>
  )
}

function AislePanel({ aisle }: { aisle: AisleStatus }) {
  const [open, setOpen] = useState(false)
  return (
    <div className="border border-slate-200 rounded-lg overflow-hidden">
      <button onClick={() => setOpen(o => !o)}
        className="w-full flex items-center justify-between px-4 py-3 bg-slate-50 hover:bg-slate-100 transition-colors">
        <div className="flex items-center gap-3">
          <span className="font-mono font-bold text-indigo-600 text-lg">Pasillo {aisle.aisleCode}</span>
          <span className="text-xs text-slate-500">{num(aisle.currentStock)} / {num(aisle.totalCapacity)} u.</span>
        </div>
        <div className="flex items-center gap-3 w-48">
          <OccupancyBar value={aisle.occupancyPercentage} />
          <span className="text-slate-400 text-sm">{open ? '▲' : '▼'}</span>
        </div>
      </button>
      {open && (
        <table className="w-full text-xs">
          <thead className="text-slate-500 bg-white border-b border-slate-100">
            <tr>
              {['Ubicación', 'Stock', 'Espacio', 'Ocupación', 'SKUs'].map(h => (
                <th key={h} className="px-4 py-2 text-left font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {aisle.locations.filter(l => l.currentStock > 0 || true).map(loc => (
              <tr key={loc.locationCode} className={loc.currentStock > 0 ? 'bg-white' : 'bg-slate-50 text-slate-400'}>
                <td className="px-4 py-2 font-mono font-medium">{loc.locationCode}</td>
                <td className="px-4 py-2">{num(loc.currentStock)} u.</td>
                <td className="px-4 py-2">{num(loc.availableSpace)} u.</td>
                <td className="px-4 py-2 w-36"><OccupancyBar value={loc.occupancyPercentage} /></td>
                <td className="px-4 py-2">
                  <div className="flex flex-wrap gap-1">
                    {loc.skusPresent.map(s => (
                      <span key={s} className="bg-indigo-100 text-indigo-700 rounded px-1.5 py-0.5">{s}</span>
                    ))}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default function WmsPage() {
  const [refs, setRefs] = useState<ProductReferenceDto[]>([])
  const [selectedRef, setSelectedRef] = useState<ProductReferenceDto | null>(null)
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const [status, setStatus] = useState<WarehouseStatusResult | null>(null)
  const [result, setResult] = useState<AssignStockResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const loadStatus = () => getWarehouseStatus().then(setStatus).catch(console.error)

  useEffect(() => {
    getProductReferences().then(r => {
      setRefs(r)
      if (r[0]) handleSelectRef(r[0])
    }).catch(console.error)
    loadStatus()
  }, [])

  const handleSelectRef = (ref: ProductReferenceDto) => {
    setSelectedRef(ref)
    const q: Record<string, string> = {}
    ref.variants.forEach(v => { q[v.id] = '100' })
    setQuantities(q)
    setResult(null)
    setError('')
  }

  const handleAssign = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRef) return
    setError('')
    setSubmitting(true)
    try {
      const res = await assignStock({
        productReferenceId: selectedRef.id,
        items: selectedRef.variants
          .filter(v => parseInt(quantities[v.id] || '0') > 0)
          .map(v => ({ variantId: v.id, quantity: parseInt(quantities[v.id]) })),
      })
      setResult(res)
      loadStatus()
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="p-8 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">WMS — Gestión de Bodega</h1>
        <p className="text-sm text-slate-500 mt-1">Asignación automática de ubicaciones físicas (Pasillo-Estante-Nivel)</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8">
        {/* Formulario de asignación */}
        <div className="space-y-4">
          <form onSubmit={handleAssign} className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 space-y-4">
            <h2 className="font-semibold text-slate-700">Ingresar Stock a Bodega</h2>

            {error && <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">{error}</div>}

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">Referencia</label>
              <select
                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-amber-300"
                value={selectedRef?.id ?? ''}
                onChange={e => { const r = refs.find(x => x.id === e.target.value); if (r) handleSelectRef(r) }}>
                {refs.map(r => <option key={r.id} value={r.id}>{r.referenceCode} – {r.name}</option>)}
              </select>
            </div>

            {selectedRef && (
              <div>
                <label className="block text-xs font-medium text-slate-600 mb-2">Cantidades por variante</label>
                <div className="space-y-2">
                  {selectedRef.variants.map(v => (
                    <div key={v.id} className="flex items-center gap-3">
                      <span className="w-3 h-3 rounded-full border border-slate-200 shrink-0"
                        style={{ backgroundColor: v.colorHex }} />
                      <span className="font-mono text-xs text-indigo-600 w-32 shrink-0">{v.sku}</span>
                      <span className="text-xs text-slate-500 w-8">{v.sizeCode}</span>
                      <input type="number" min="0" value={quantities[v.id] ?? ''}
                        onChange={e => setQuantities(q => ({ ...q, [v.id]: e.target.value }))}
                        className="w-24 rounded border border-slate-200 px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-amber-300" />
                      <span className="text-xs text-slate-400">unidades</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <button type="submit" disabled={submitting || !selectedRef}
              className="w-full rounded-lg bg-amber-500 text-white py-2.5 text-sm font-medium
                         hover:bg-amber-600 disabled:opacity-50 transition-colors">
              {submitting ? 'Asignando...' : 'Asignar a Bodega'}
            </button>
          </form>

          {/* Resultado de asignación */}
          {result && (
            <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold text-slate-700">Asignación completada</h3>
                <span className="text-xs text-slate-400">{result.totalUnitsAssigned} u. · {result.locationsUsed} ubicaciones</span>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-xs">
                  <thead className="text-slate-500 uppercase">
                    <tr>
                      {['Ubicación', 'SKU', 'Talla', 'Cantidad', 'Espacio rest.'].map(h => (
                        <th key={h} className="pb-2 text-left font-medium">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {result.assignments.map((a, i) => (
                      <tr key={i}>
                        <td className="py-1.5 pr-2 font-mono font-bold text-amber-700">{a.locationCode}</td>
                        <td className="py-1.5 pr-2 text-indigo-600">{a.sku}</td>
                        <td className="py-1.5 pr-2">{a.size}</td>
                        <td className="py-1.5 pr-2 font-medium">{num(a.quantityAssigned)}</td>
                        <td className="py-1.5 text-slate-500">{num(a.locationRemainingSpace)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        {/* Estado de bodega */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-slate-100">
            <div className="flex items-center justify-between">
              <h2 className="font-semibold text-slate-700">Estado de Bodega</h2>
              {status && (
                <span className="text-xs text-slate-500">
                  {num(status.totalStockUnits)} u. · {pct(status.occupancyPercentage)} ocupado
                </span>
              )}
            </div>
            {status && (
              <div className="mt-2">
                <OccupancyBar value={status.occupancyPercentage} />
              </div>
            )}
          </div>
          <div className="p-4 space-y-2">
            {!status ? (
              <div className="text-center text-slate-400 text-sm py-8">Cargando...</div>
            ) : (
              status.aisles.map(aisle => <AislePanel key={aisle.aisleCode} aisle={aisle} />)
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
