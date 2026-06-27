'use client'
import { useEffect, useState } from 'react'
import { calculateProductionCost, getFinancialInventory } from '@/lib/api/sag'
import { getProductReferences } from '@/lib/api/plm'
import type { ProductReferenceDto, ProductionCostResult, FinancialInventoryResult } from '@/types/api'
import { cop, num, pct } from '@/lib/utils/format'

export default function SagPage() {
  const [refs, setRefs] = useState<ProductReferenceDto[]>([])
  const [selectedRef, setSelectedRef] = useState<ProductReferenceDto | null>(null)
  const [overhead, setOverhead] = useState('18.5')
  const [laborCosts, setLaborCosts] = useState<Record<string, string>>({})
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const [result, setResult] = useState<ProductionCostResult | null>(null)
  const [inventory, setInventory] = useState<FinancialInventoryResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    getProductReferences().then(r => { setRefs(r); if (r[0]) handleSelectRef(r[0]) }).catch(console.error)
    getFinancialInventory().then(setInventory).catch(console.error)
  }, [])

  const handleSelectRef = (ref: ProductReferenceDto) => {
    setSelectedRef(ref)
    const q: Record<string, string> = {}
    const l: Record<string, string> = {}
    ref.variants.forEach(v => { q[v.id] = '100'; l[v.id] = '6500' })
    setQuantities(q)
    setLaborCosts(l)
    setResult(null)
    setError('')
  }

  const handleCalculate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRef) return
    setError('')
    setSubmitting(true)
    try {
      const res = await calculateProductionCost({
        productReferenceId: selectedRef.id,
        overheadPercentage: parseFloat(overhead),
        items: selectedRef.variants
          .filter(v => parseInt(quantities[v.id] || '0') > 0)
          .map(v => ({
            variantId: v.id,
            quantity: parseInt(quantities[v.id]),
            laborCostPerUnit: parseFloat(laborCosts[v.id] || '0'),
          })),
      })
      setResult(res)
      getFinancialInventory().then(setInventory).catch(console.error)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="p-8 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">SAG — Administración General</h1>
        <p className="text-sm text-slate-500 mt-1">Costos de producción e inventario financiero valorizado</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8">
        {/* Calculador */}
        <div className="space-y-4">
          <form onSubmit={handleCalculate} className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 space-y-4">
            <h2 className="font-semibold text-slate-700">Calculador de Costo de Producción</h2>

            {error && <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">{error}</div>}

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-slate-600 mb-1">Referencia</label>
                <select
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-300"
                  value={selectedRef?.id ?? ''}
                  onChange={e => { const r = refs.find(x => x.id === e.target.value); if (r) handleSelectRef(r) }}>
                  {refs.map(r => <option key={r.id} value={r.id}>{r.referenceCode} – {r.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-slate-600 mb-1">CIF (overhead %)</label>
                <input type="number" step="0.1" min="0" max="100" value={overhead}
                  onChange={e => setOverhead(e.target.value)}
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-300" />
              </div>
            </div>

            {selectedRef && (
              <div className="overflow-x-auto">
                <table className="w-full text-xs">
                  <thead className="text-slate-500 uppercase tracking-wider">
                    <tr>
                      {['SKU', 'Talla', 'Color', 'Cantidad', 'MO/unidad'].map(h => (
                        <th key={h} className="pb-2 text-left font-medium">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {selectedRef.variants.map(v => (
                      <tr key={v.id}>
                        <td className="py-1.5 pr-2 font-mono text-indigo-600">{v.sku}</td>
                        <td className="py-1.5 pr-2">{v.sizeCode}</td>
                        <td className="py-1.5 pr-2 flex items-center gap-1">
                          <span className="w-3 h-3 rounded-full inline-block border border-slate-200"
                            style={{ backgroundColor: v.colorHex }} />
                          {v.colorName}
                        </td>
                        <td className="py-1.5 pr-2">
                          <input type="number" min="0" value={quantities[v.id] ?? ''}
                            onChange={e => setQuantities(q => ({ ...q, [v.id]: e.target.value }))}
                            className="w-20 rounded border border-slate-200 px-2 py-1 focus:outline-none focus:ring-1 focus:ring-emerald-300" />
                        </td>
                        <td className="py-1.5">
                          <input type="number" min="0" value={laborCosts[v.id] ?? ''}
                            onChange={e => setLaborCosts(l => ({ ...l, [v.id]: e.target.value }))}
                            className="w-24 rounded border border-slate-200 px-2 py-1 focus:outline-none focus:ring-1 focus:ring-emerald-300" />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <button type="submit" disabled={submitting || !selectedRef}
              className="w-full rounded-lg bg-emerald-600 text-white py-2.5 text-sm font-medium
                         hover:bg-emerald-700 disabled:opacity-50 transition-colors">
              {submitting ? 'Calculando...' : 'Calcular Costo'}
            </button>
          </form>

          {/* Resultado */}
          {result && (
            <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold text-slate-700">Resultado — {result.orderCode}</h3>
                <span className="text-xs text-slate-400">{result.totalUnits} u. · CIF {result.overheadPercentage}%</span>
              </div>
              <div className="grid grid-cols-2 gap-3">
                {[
                  { l: 'Material', v: result.summary.totalMaterialCost, c: 'text-slate-700' },
                  { l: 'Mano de obra', v: result.summary.totalLaborCost, c: 'text-slate-700' },
                  { l: 'CIF', v: result.summary.totalOverheadCost, c: 'text-slate-700' },
                  { l: 'Costo/unidad', v: result.summary.costPerUnit, c: 'text-emerald-700' },
                ].map(({ l, v, c }) => (
                  <div key={l} className="bg-slate-50 rounded-lg p-3">
                    <p className="text-xs text-slate-500">{l}</p>
                    <p className={`font-semibold text-sm ${c}`}>{cop(v)}</p>
                  </div>
                ))}
              </div>
              <div className="rounded-lg bg-emerald-50 border border-emerald-200 p-3 flex justify-between items-center">
                <span className="text-sm font-medium text-emerald-800">Gran Total</span>
                <span className="text-lg font-bold text-emerald-700">{cop(result.summary.grandTotal)}</span>
              </div>
            </div>
          )}
        </div>

        {/* Inventario Financiero */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
            <h2 className="font-semibold text-slate-700">Inventario Financiero</h2>
            {inventory && (
              <span className="text-xs text-slate-500">
                {num(inventory.totalUnitsInStock)} u. · {inventory.totalSkus} SKUs
              </span>
            )}
          </div>
          {!inventory ? (
            <div className="p-8 text-center text-slate-400 text-sm">Cargando...</div>
          ) : (
            <>
              <table className="w-full text-sm">
                <thead className="bg-slate-50 text-slate-500 text-xs uppercase">
                  <tr>
                    {['SKU', 'Talla', 'Color', 'Stock', 'Costo u.', 'Valor'].map(h => (
                      <th key={h} className="px-4 py-3 text-left font-medium">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {inventory.lines.flatMap(line =>
                    line.variants.map(v => (
                      <tr key={v.variantId} className="hover:bg-slate-50">
                        <td className="px-4 py-2.5 font-mono text-xs text-indigo-600">{v.sku}</td>
                        <td className="px-4 py-2.5">{v.size}</td>
                        <td className="px-4 py-2.5 text-xs text-slate-600">{v.color}</td>
                        <td className="px-4 py-2.5 font-medium">{num(v.stockQuantity)}</td>
                        <td className="px-4 py-2.5 text-slate-600">{cop(v.unitCost)}</td>
                        <td className="px-4 py-2.5 font-semibold text-emerald-700">{cop(v.totalValue)}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
              <div className="px-4 py-3 bg-emerald-50 border-t border-emerald-100 flex justify-between items-center">
                <span className="text-sm font-medium text-emerald-800">Valor total inventario</span>
                <span className="text-base font-bold text-emerald-700">{cop(inventory.grandTotalValue)}</span>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
