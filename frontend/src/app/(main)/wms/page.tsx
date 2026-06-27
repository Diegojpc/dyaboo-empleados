'use client'
import { useEffect, useState } from 'react'
import { assignStock, getWarehouseStatus } from '@/lib/api/wms'
import { getProductReferences } from '@/lib/api/plm'
import type { ProductReferenceDto, AssignStockResult, WarehouseStatusResult, AisleStatus } from '@/types/api'
import { num, pct } from '@/lib/utils/format'

function OccupancyBar({ value }: { value: number }) {
  const color = value >= 100 ? '#dc2626' : value >= 75 ? '#d97706' : value > 0 ? 'var(--accent)' : 'var(--border)'
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 h-2 rounded-full overflow-hidden" style={{ backgroundColor: 'var(--surface-2)' }}>
        <div className="h-full rounded-full transition-all" style={{ width: `${Math.min(value, 100)}%`, backgroundColor: color }} />
      </div>
      <span className="text-xs subtle w-12 text-right">{pct(value)}</span>
    </div>
  )
}

function AislePanel({ aisle }: { aisle: AisleStatus }) {
  const [open, setOpen] = useState(false)
  return (
    <div className="rounded-lg overflow-hidden" style={{ border: '1px solid var(--border)' }}>
      <button onClick={() => setOpen(o => !o)}
        className="w-full flex items-center justify-between px-4 py-3 transition-colors hover:opacity-90"
        style={{ backgroundColor: 'var(--surface-2)' }}>
        <div className="flex items-center gap-3">
          <span className="font-mono font-bold text-lg accent">Pasillo {aisle.aisleCode}</span>
          <span className="text-xs subtle">{num(aisle.currentStock)} / {num(aisle.totalCapacity)} u.</span>
        </div>
        <div className="flex items-center gap-3 w-48">
          <OccupancyBar value={aisle.occupancyPercentage} />
          <span className="subtle text-sm">{open ? '▲' : '▼'}</span>
        </div>
      </button>
      {open && (
        <table className="w-full text-xs">
          <thead><tr className="table-head">
            {['Ubicación', 'Stock', 'Espacio', 'Ocupación', 'SKUs'].map(h => <th key={h}>{h}</th>)}
          </tr></thead>
          <tbody>
            {aisle.locations.map(loc => (
              <tr key={loc.locationCode} className="table-row" style={{ opacity: loc.currentStock > 0 ? 1 : 0.5 }}>
                <td className="px-4 py-2 font-mono font-medium">{loc.locationCode}</td>
                <td className="px-4 py-2">{num(loc.currentStock)} u.</td>
                <td className="px-4 py-2">{num(loc.availableSpace)} u.</td>
                <td className="px-4 py-2 w-36"><OccupancyBar value={loc.occupancyPercentage} /></td>
                <td className="px-4 py-2">
                  <div className="flex flex-wrap gap-1">
                    {loc.skusPresent.map(s => <span key={s} className="badge text-[10px]">{s}</span>)}
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
  const [refs, setRefs]             = useState<ProductReferenceDto[]>([])
  const [selectedRef, setSelectedRef] = useState<ProductReferenceDto | null>(null)
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const [status, setStatus]         = useState<WarehouseStatusResult | null>(null)
  const [result, setResult]         = useState<AssignStockResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError]           = useState('')

  const loadStatus = () => getWarehouseStatus().then(setStatus).catch(console.error)

  useEffect(() => {
    getProductReferences().then(r => { setRefs(r); if (r[0]) handleSelectRef(r[0]) }).catch(console.error)
    loadStatus()
  }, [])

  const handleSelectRef = (ref: ProductReferenceDto) => {
    setSelectedRef(ref)
    const q: Record<string, string> = {}
    ref.variants.forEach(v => { q[v.id] = '100' })
    setQuantities(q); setResult(null); setError('')
  }

  const handleAssign = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRef) return
    setError(''); setSubmitting(true)
    try {
      const res = await assignStock({
        productReferenceId: selectedRef.id,
        items: selectedRef.variants.filter(v => parseInt(quantities[v.id] || '0') > 0)
          .map(v => ({ variantId: v.id, quantity: parseInt(quantities[v.id]) })),
      })
      setResult(res); loadStatus()
    } catch (err) { setError((err as Error).message) }
    finally { setSubmitting(false) }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>WMS — Gestión de Bodega</h1>
        <p>Asignación automática de ubicaciones físicas (Pasillo-Estante-Nivel)</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Formulario */}
        <div className="space-y-4">
          <form onSubmit={handleAssign} className="card p-6 space-y-4">
            <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Ingresar Stock a Bodega</h2>

            {error && <div className="rounded-lg px-4 py-3 text-sm" style={{ backgroundColor: 'rgba(139,0,0,0.1)', border: '1px solid rgba(139,0,0,0.3)', color: '#dc2626' }}>{error}</div>}

            <div>
              <label className="block text-xs font-medium mb-1 muted">Referencia</label>
              <select className="input" value={selectedRef?.id ?? ''}
                onChange={e => { const r = refs.find(x => x.id === e.target.value); if (r) handleSelectRef(r) }}>
                {refs.map(r => <option key={r.id} value={r.id}>{r.referenceCode} – {r.name}</option>)}
              </select>
            </div>

            {selectedRef && (
              <div>
                <label className="block text-xs font-medium mb-2 muted">Cantidades por variante</label>
                <div className="space-y-2">
                  {selectedRef.variants.map(v => (
                    <div key={v.id} className="flex items-center gap-3">
                      <span className="w-3 h-3 rounded-full border shrink-0" style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
                      <span className="font-mono text-xs accent w-32 shrink-0">{v.sku}</span>
                      <span className="text-xs muted w-8">{v.sizeCode}</span>
                      <input type="number" min="0" value={quantities[v.id] ?? ''}
                        onChange={e => setQuantities(q => ({ ...q, [v.id]: e.target.value }))}
                        className="input w-24 py-1 px-2 text-sm" />
                      <span className="text-xs subtle">unidades</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <button type="submit" disabled={submitting || !selectedRef} className="btn-primary w-full">
              {submitting ? 'Asignando...' : 'Asignar a Bodega'}
            </button>
          </form>

          {/* Resultado */}
          {result && (
            <div className="card p-6 space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold" style={{ color: 'var(--text)' }}>Asignación completada</h3>
                <span className="text-xs subtle">{result.totalUnitsAssigned} u. · {result.locationsUsed} ubicaciones</span>
              </div>
              <table className="w-full text-xs">
                <thead><tr className="table-head">
                  {['Ubicación', 'SKU', 'Talla', 'Cantidad', 'Espacio rest.'].map(h => <th key={h}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {result.assignments.map((a, i) => (
                    <tr key={i} className="table-row">
                      <td className="px-3 py-1.5 font-mono font-bold accent">{a.locationCode}</td>
                      <td className="px-3 py-1.5 font-mono accent">{a.sku}</td>
                      <td className="px-3 py-1.5">{a.size}</td>
                      <td className="px-3 py-1.5 font-medium">{num(a.quantityAssigned)}</td>
                      <td className="px-3 py-1.5 muted">{num(a.locationRemainingSpace)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Estado de bodega */}
        <div className="card overflow-hidden">
          <div className="card-header">
            <div className="flex items-center justify-between">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Estado de Bodega</h2>
              {status && <span className="text-xs muted">{num(status.totalStockUnits)} u. · {pct(status.occupancyPercentage)}</span>}
            </div>
            {status && <div className="mt-2"><OccupancyBar value={status.occupancyPercentage} /></div>}
          </div>
          <div className="p-4 space-y-2">
            {!status ? (
              <div className="text-center subtle text-sm py-8">Cargando...</div>
            ) : (
              status.aisles.map(aisle => <AislePanel key={aisle.aisleCode} aisle={aisle} />)
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
