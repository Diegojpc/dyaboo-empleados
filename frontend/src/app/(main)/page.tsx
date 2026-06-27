'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'
import { getFinancialInventory } from '@/lib/api/sag'
import { getWarehouseStatus } from '@/lib/api/wms'
import { getProductReferences } from '@/lib/api/plm'
import { cop, num, pct } from '@/lib/utils/format'
import type { FinancialInventoryResult, WarehouseStatusResult, ProductReferenceDto } from '@/types/api'

function StatCard({ label, value, sub, accent }: {
  label: string; value: string; sub?: string; accent?: boolean
}) {
  return (
    <div className="stat-card">
      <p className="text-xs font-medium uppercase tracking-wider muted">{label}</p>
      <p className="mt-1 text-2xl font-bold" style={{ color: accent ? 'var(--accent)' : 'var(--text)' }}>{value}</p>
      {sub && <p className="mt-1 text-xs subtle">{sub}</p>}
    </div>
  )
}

const MODULE_CARDS = [
  { href: '/plm', title: 'PLM', desc: 'Gestión del Ciclo de Vida del Producto', icon: '◈' },
  { href: '/sag', title: 'SAG', desc: 'Administración y Costos de Producción',  icon: '◉' },
  { href: '/wms', title: 'WMS', desc: 'Gestión de Bodega y Ubicaciones',        icon: '◫' },
]

export default function DashboardPage() {
  const [sag,  setSag]  = useState<FinancialInventoryResult | null>(null)
  const [wms,  setWms]  = useState<WarehouseStatusResult | null>(null)
  const [refs, setRefs] = useState<ProductReferenceDto[]>([])

  useEffect(() => {
    getFinancialInventory().then(setSag).catch(console.error)
    getWarehouseStatus().then(setWms).catch(console.error)
    getProductReferences().then(setRefs).catch(console.error)
  }, [])

  return (
    <div className="p-8 space-y-8">
      <div className="page-header">
        <h1>Inicio</h1>
        <p>Vista general del sistema Dyaboo Empleados</p>
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
          accent
        />
        <StatCard
          label="Ocupación bodega"
          value={pct(wms?.occupancyPercentage ?? 0)}
          sub={`${wms?.occupiedLocations ?? 0} / ${wms?.totalLocations ?? 0} ubicaciones`}
          accent
        />
      </div>

      {/* Módulos */}
      <div>
        <h2 className="text-xs font-semibold uppercase tracking-wider muted mb-3">Módulos</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {MODULE_CARDS.map(({ href, title, desc, icon }) => (
            <Link key={href} href={href}
              className="flex items-start gap-4 rounded-xl p-5 transition-shadow hover:shadow-md"
              style={{ backgroundColor: 'var(--surface-2)', border: '1px solid var(--border)' }}>
              <span className="text-2xl" style={{ color: 'var(--accent)' }}>{icon}</span>
              <div>
                <p className="font-semibold" style={{ color: 'var(--text)' }}>{title}</p>
                <p className="text-xs mt-0.5 muted">{desc}</p>
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* Referencias recientes */}
      {refs.length > 0 && (
        <div>
          <h2 className="text-xs font-semibold uppercase tracking-wider muted mb-3">Referencias recientes</h2>
          <div className="card overflow-hidden">
            <table className="w-full text-sm">
              <thead><tr className="table-head">
                {['Código', 'Nombre', 'Categoría', 'Variantes', 'Stock total'].map(h => (
                  <th key={h}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {refs.map(r => (
                  <tr key={r.id} className="table-row">
                    <td className="px-4 py-3 font-mono text-xs accent">{r.referenceCode}</td>
                    <td className="px-4 py-3 font-medium">{r.name}</td>
                    <td className="px-4 py-3"><span className="badge">{r.category}</span></td>
                    <td className="px-4 py-3 muted">{r.variants.length}</td>
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
