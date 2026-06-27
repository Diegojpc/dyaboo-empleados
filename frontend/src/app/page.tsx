'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'
import { getFinancialInventory } from '@/lib/api/sag'
import { getWarehouseStatus } from '@/lib/api/wms'
import { getProductReferences } from '@/lib/api/plm'
import { cop, num, pct } from '@/lib/utils/format'
import type { FinancialInventoryResult, WarehouseStatusResult, ProductReferenceDto } from '@/types/api'

function StatCard({ label, value, sub, accent }: {
  label: string; value: string; sub?: string; accent?: string
}) {
  return (
    <div className="rounded-xl bg-white shadow-sm border border-slate-100 p-5">
      <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">{label}</p>
      <p className={`mt-1 text-2xl font-bold ${accent ?? 'text-slate-800'}`}>{value}</p>
      {sub && <p className="mt-1 text-xs text-slate-400">{sub}</p>}
    </div>
  )
}

const MODULE_CARDS = [
  { href: '/plm', title: 'PLM', desc: 'Gestión del Ciclo de Vida del Producto', color: 'bg-indigo-50 border-indigo-200', icon: '◈' },
  { href: '/sag', title: 'SAG', desc: 'Administración y Costos de Producción',  color: 'bg-emerald-50 border-emerald-200', icon: '◉' },
  { href: '/wms', title: 'WMS', desc: 'Gestión de Bodega y Ubicaciones',        color: 'bg-amber-50  border-amber-200',   icon: '◫' },
]

export default function DashboardPage() {
  const [sag, setSag] = useState<FinancialInventoryResult | null>(null)
  const [wms, setWms] = useState<WarehouseStatusResult | null>(null)
  const [refs, setRefs] = useState<ProductReferenceDto[]>([])

  useEffect(() => {
    getFinancialInventory().then(setSag).catch(console.error)
    getWarehouseStatus().then(setWms).catch(console.error)
    getProductReferences().then(setRefs).catch(console.error)
  }, [])

  return (
    <div className="p-8 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Dashboard</h1>
        <p className="text-sm text-slate-500 mt-1">Vista general del sistema Dyaboo ERP</p>
      </div>

      {/* Métricas */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Referencias activas"
          value={num(refs.length)}
          sub={`${refs.reduce((a, r) => a + r.variants.length, 0)} variantes totales`}
        />
        <StatCard
          label="Unidades en stock"
          value={num(sag?.totalUnitsInStock ?? 0)}
          sub={`${sag?.totalSkus ?? 0} SKUs activos`}
        />
        <StatCard
          label="Valor inventario"
          value={cop(sag?.grandTotalValue ?? 0)}
          sub="Costo de materia prima"
          accent="text-emerald-600"
        />
        <StatCard
          label="Ocupación bodega"
          value={pct(wms?.occupancyPercentage ?? 0)}
          sub={`${wms?.occupiedLocations ?? 0} / ${wms?.totalLocations ?? 0} ubicaciones`}
          accent={
            (wms?.occupancyPercentage ?? 0) > 80 ? 'text-red-600' :
            (wms?.occupancyPercentage ?? 0) > 50 ? 'text-amber-600' : 'text-indigo-600'
          }
        />
      </div>

      {/* Módulos */}
      <div>
        <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-3">Módulos</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {MODULE_CARDS.map(({ href, title, desc, color, icon }) => (
            <Link key={href} href={href}
              className={`flex items-start gap-4 rounded-xl border p-5 ${color} hover:shadow-md transition-shadow`}>
              <span className="text-2xl">{icon}</span>
              <div>
                <p className="font-semibold text-slate-800">{title}</p>
                <p className="text-xs text-slate-600 mt-0.5">{desc}</p>
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* Referencias recientes */}
      {refs.length > 0 && (
        <div>
          <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-3">
            Referencias recientes
          </h2>
          <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-slate-50 text-slate-500 text-xs uppercase tracking-wider">
                <tr>
                  {['Código', 'Nombre', 'Categoría', 'Variantes', 'Stock total'].map(h => (
                    <th key={h} className="px-4 py-3 text-left font-medium">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {refs.map(r => (
                  <tr key={r.id} className="hover:bg-slate-50">
                    <td className="px-4 py-3 font-mono text-xs text-indigo-600">{r.referenceCode}</td>
                    <td className="px-4 py-3 font-medium">{r.name}</td>
                    <td className="px-4 py-3">
                      <span className="rounded-full bg-indigo-100 text-indigo-700 px-2 py-0.5 text-xs">
                        {r.category}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-slate-600">{r.variants.length}</td>
                    <td className="px-4 py-3 font-medium">
                      {num(r.variants.reduce((a, v) => a + v.stockQuantity, 0))} u.
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
