'use client'
import { useEffect, useState } from 'react'
import { calculateProductionCost, getFinancialInventory } from '@/lib/api/sag'
import { getProductReferences } from '@/lib/api/plm'
import type { ProductReferenceDto, ProductionCostResult, FinancialInventoryResult } from '@/types/api'
import { cop, num } from '@/lib/utils/format'

export default function SagPage() {
  const [refs, setRefs]             = useState<ProductReferenceDto[]>([])
  const [selectedRef, setSelectedRef] = useState<ProductReferenceDto | null>(null)
  const [overhead, setOverhead]     = useState('18.5')
  const [laborCosts, setLaborCosts] = useState<Record<string, string>>({})
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const [result, setResult]         = useState<ProductionCostResult | null>(null)
  const [inventory, setInventory]   = useState<FinancialInventoryResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError]           = useState('')

  useEffect(() => {
    getProductReferences().then(r => { setRefs(r); if (r[0]) handleSelectRef(r[0]) }).catch(console.error)
    getFinancialInventory().then(setInventory).catch(console.error)
  }, [])

  const handleSelectRef = (ref: ProductReferenceDto) => {
    setSelectedRef(ref)
    const q: Record<string, string> = {}
    const l: Record<string, string> = {}
    ref.variants.forEach(v => { q[v.id] = '100'; l[v.id] = '6500' })
    setQuantities(q); setLaborCosts(l); setResult(null); setError('')
  }

  const handleCalculate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRef) return
    setError(''); setSubmitting(true)
    try {
      const res = await calculateProductionCost({
        productReferenceId: selectedRef.id,
        overheadPercentage: parseFloat(overhead),
        items: selectedRef.variants
          .filter(v => parseInt(quantities[v.id] || '0') > 0)
          .map(v => ({ variantId: v.id, quantity: parseInt(quantities[v.id]), laborCostPerUnit: parseFloat(laborCosts[v.id] || '0') })),
      })
      setResult(res)
      getFinancialInventory().then(setInventory).catch(console.error)
    } catch (err) { setError((err as Error).message) }
    finally { setSubmitting(false) }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>SAG — Administración General</h1>
        <p>Costos de producción e inventario financiero valorizado</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Calculador */}
        <div className="space-y-4">
          <form onSubmit={handleCalculate} className="card p-6 space-y-4">
            <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Calculador de Costo de Producción</h2>

            {error && <div className="rounded-lg px-4 py-3 text-sm" style={{ backgroundColor: 'rgba(139,0,0,0.1)', border: '1px solid rgba(139,0,0,0.3)', color: '#dc2626' }}>{error}</div>}

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium mb-1 muted">Referencia</label>
                <select className="input" value={selectedRef?.id ?? ''}
                  onChange={e => { const r = refs.find(x => x.id === e.target.value); if (r) handleSelectRef(r) }}>
                  {refs.map(r => <option key={r.id} value={r.id}>{r.referenceCode} – {r.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium mb-1 muted">CIF (overhead %)</label>
                <input type="number" step="0.1" min="0" max="100" value={overhead}
                  onChange={e => setOverhead(e.target.value)} className="input" />
              </div>
            </div>

            {selectedRef && (
              <div className="overflow-x-auto">
                <table className="w-full text-xs">
                  <thead><tr className="table-head">
                    {['SKU', 'Talla', 'Color', 'Cantidad', 'MO/unidad'].map(h => <th key={h}>{h}</th>)}
                  </tr></thead>
                  <tbody>
                    {selectedRef.variants.map(v => (
                      <tr key={v.id} className="table-row">
                        <td className="px-3 py-1.5 font-mono accent">{v.sku}</td>
                        <td className="px-3 py-1.5">{v.sizeCode}</td>
                        <td className="px-3 py-1.5">
                          <div className="flex items-center gap-1.5">
                            <span className="w-3 h-3 rounded-full inline-block border" style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
                            {v.colorName}
                          </div>
                        </td>
                        <td className="px-3 py-1.5">
                          <input type="number" min="0" value={quantities[v.id] ?? ''} onChange={e => setQuantities(q => ({ ...q, [v.id]: e.target.value }))}
                            className="input w-20 py-1 px-2 text-xs" />
                        </td>
                        <td className="px-3 py-1.5">
                          <input type="number" min="0" value={laborCosts[v.id] ?? ''} onChange={e => setLaborCosts(l => ({ ...l, [v.id]: e.target.value }))}
                            className="input w-24 py-1 px-2 text-xs" />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <button type="submit" disabled={submitting || !selectedRef} className="btn-primary w-full">
              {submitting ? 'Calculando...' : 'Calcular Costo'}
            </button>
          </form>

          {/* Resultado */}
          {result && (
            <div className="card p-6 space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold" style={{ color: 'var(--text)' }}>Resultado — {result.orderCode}</h3>
                <span className="text-xs subtle">{result.totalUnits} u. · CIF {result.overheadPercentage}%</span>
              </div>
              <div className="grid grid-cols-2 gap-3">
                {[
                  { l: 'Material',     v: result.summary.totalMaterialCost },
                  { l: 'Mano de obra', v: result.summary.totalLaborCost },
                  { l: 'CIF',          v: result.summary.totalOverheadCost },
                  { l: 'Costo/unidad', v: result.summary.costPerUnit },
                ].map(({ l, v }) => (
                  <div key={l} className="rounded-lg p-3" style={{ backgroundColor: 'var(--surface-2)' }}>
                    <p className="text-xs muted">{l}</p>
                    <p className="font-semibold text-sm" style={{ color: 'var(--text)' }}>{cop(v)}</p>
                  </div>
                ))}
              </div>
              <div className="rounded-lg p-3 flex justify-between items-center" style={{ backgroundColor: 'var(--accent-light)', border: '1px solid var(--accent)' }}>
                <span className="text-sm font-medium accent">Gran Total</span>
                <span className="text-lg font-bold accent">{cop(result.summary.grandTotal)}</span>
              </div>
            </div>
          )}
        </div>

        {/* Inventario */}
        <div className="card overflow-hidden">
          <div className="card-header flex items-center justify-between">
            <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Inventario Financiero</h2>
            {inventory && <span className="text-xs muted">{num(inventory.totalUnitsInStock)} u. · {inventory.totalSkus} SKUs</span>}
          </div>
          {!inventory ? (
            <div className="p-8 text-center subtle text-sm">Cargando...</div>
          ) : (
            <>
              <table className="w-full">
                <thead><tr className="table-head">
                  {['SKU', 'Talla', 'Color', 'Stock', 'Costo u.', 'Valor'].map(h => <th key={h}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {inventory.lines.flatMap(line => line.variants.map(v => (
                    <tr key={v.variantId} className="table-row">
                      <td className="px-3 py-1.5 font-mono accent">{v.sku}</td>
                      <td className="px-3 py-1.5">{v.size}</td>
                      <td className="px-3 py-1.5">
                        <div className="flex items-center gap-1.5">
                          <span className="w-3 h-3 rounded-full shrink-0 border"
                            style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
                          <span className="muted">{v.colorName}</span>
                        </div>
                      </td>
                      <td className="px-3 py-1.5 font-medium">{num(v.stockQuantity)}</td>
                      <td className="px-3 py-1.5 muted">{cop(v.unitCost)}</td>
                      <td className="px-3 py-1.5 font-semibold accent">{cop(v.totalValue)}</td>
                    </tr>
                  )))}
                </tbody>
              </table>
              <div className="px-4 py-3 flex justify-between items-center" style={{ backgroundColor: 'var(--accent-light)', borderTop: '1px solid var(--accent)' }}>
                <span className="text-sm font-medium accent">Valor total</span>
                <span className="text-base font-bold accent">{cop(inventory.grandTotalValue)}</span>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
