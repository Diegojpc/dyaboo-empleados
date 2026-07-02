'use client'
import { useEffect, useState } from 'react'
import {
  getConfeccionistas, createConfeccionista,
  getCuttingOrders, createCuttingOrder, completeCuttingOrder,
  getSewingOrders, createSewingOrder, receiveSewingOrder,
} from '@/lib/api/produccion'
import { getProductReferences } from '@/lib/api/plm'
import type { ProductReferenceDto, ConfeccionistaDto, CuttingOrderDto, SewingOrderDto } from '@/types/api'
import { num } from '@/lib/utils/format'

function StatusBadge({ status }: { status: string }) {
  const styles: Record<string, { bg: string; fg: string; label: string }> = {
    InProgress: { bg: 'rgba(217,119,6,0.12)',  fg: '#d97706', label: 'En corte' },
    Completed:  { bg: 'rgba(22,163,74,0.12)',  fg: '#16a34a', label: 'Cortada' },
    Assigned:   { bg: 'rgba(217,119,6,0.12)',  fg: '#d97706', label: 'En taller' },
    Received:   { bg: 'rgba(22,163,74,0.12)',  fg: '#16a34a', label: 'Recibida' },
  }
  const s = styles[status] ?? { bg: 'var(--surface-2)', fg: 'var(--text)', label: status }
  return (
    <span className="text-[10px] font-semibold px-2 py-0.5 rounded-full"
      style={{ backgroundColor: s.bg, color: s.fg }}>{s.label}</span>
  )
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

// ── Panel por orden de corte ────────────────────────────────────────────────
function CuttingOrderRow({ order, talleres, onChanged }: {
  order: CuttingOrderDto
  talleres: ConfeccionistaDto[]
  onChanged: () => void
}) {
  const [open, setOpen]         = useState(false)
  const [cutQty, setCutQty]     = useState<Record<string, string>>(
    () => Object.fromEntries(order.items.map(i => [i.id, String(i.plannedQuantity)])))
  const [tallerId, setTallerId] = useState(talleres[0]?.id ?? '')
  const [busy, setBusy]         = useState(false)
  const [error, setError]       = useState('')

  const handleComplete = async () => {
    setError(''); setBusy(true)
    try {
      await completeCuttingOrder(order.id, order.items.map(i => ({
        itemId: i.id, cutQuantity: parseInt(cutQty[i.id] || '0'),
      })))
      onChanged()
    } catch (err) { setError((err as Error).message) }
    finally { setBusy(false) }
  }

  const handleSendToTaller = async () => {
    if (!tallerId) return
    setError(''); setBusy(true)
    try {
      await createSewingOrder({ cuttingOrderId: order.id, confeccionistaId: tallerId })
      onChanged()
    } catch (err) { setError((err as Error).message) }
    finally { setBusy(false) }
  }

  return (
    <div className="rounded-lg overflow-hidden" style={{ border: '1px solid var(--border)' }}>
      <button onClick={() => setOpen(o => !o)}
        className="w-full flex items-center justify-between px-4 py-3 transition-colors hover:opacity-90"
        style={{ backgroundColor: 'var(--surface-2)' }}>
        <div className="flex items-center gap-3">
          <span className="font-mono font-bold text-sm accent">{order.orderCode}</span>
          <span className="text-xs muted">{order.productReferenceName}</span>
          <StatusBadge status={order.status} />
        </div>
        <div className="flex items-center gap-3">
          <span className="text-xs subtle">
            {order.status === 'InProgress'
              ? `${num(order.totalPlannedUnits)} u. planeadas`
              : `${num(order.totalCutUnits)} u. cortadas`}
          </span>
          <span className="subtle text-sm">{open ? '▲' : '▼'}</span>
        </div>
      </button>

      {open && (
        <div className="p-4 space-y-3">
          <ErrorBox message={error} />
          <table className="w-full text-xs">
            <thead><tr className="table-head">
              {['SKU', 'Talla', 'Color', 'Planeadas', 'Cortadas'].map(h => <th key={h}>{h}</th>)}
            </tr></thead>
            <tbody>
              {order.items.map(i => (
                <tr key={i.id} className="table-row">
                  <td className="px-3 py-1.5 font-mono accent">{i.sku}</td>
                  <td className="px-3 py-1.5">{i.size}</td>
                  <td className="px-3 py-1.5">{i.color}</td>
                  <td className="px-3 py-1.5">{num(i.plannedQuantity)}</td>
                  <td className="px-3 py-1.5">
                    {order.status === 'InProgress' ? (
                      <input type="number" min="0" value={cutQty[i.id] ?? ''}
                        onChange={e => setCutQty(q => ({ ...q, [i.id]: e.target.value }))}
                        className="input w-20 py-1 px-2 text-xs" />
                    ) : <span className="font-medium">{num(i.cutQuantity)}</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {order.status === 'InProgress' && (
            <button onClick={handleComplete} disabled={busy} className="btn-primary w-full">
              {busy ? 'Registrando...' : 'Completar corte'}
            </button>
          )}

          {order.status === 'Completed' && !order.hasSewingOrder && (
            <div className="flex items-center gap-2">
              <select className="input flex-1" value={tallerId} onChange={e => setTallerId(e.target.value)}>
                {talleres.map(t => <option key={t.id} value={t.id}>{t.name} — {t.city}</option>)}
              </select>
              <button onClick={handleSendToTaller} disabled={busy || !tallerId} className="btn-primary shrink-0">
                {busy ? 'Enviando...' : 'Enviar a confección'}
              </button>
            </div>
          )}

          {order.status === 'Completed' && order.hasSewingOrder && (
            <p className="text-xs subtle">Ya enviada a confección.</p>
          )}
        </div>
      )}
    </div>
  )
}

// ── Panel por orden de confección ───────────────────────────────────────────
function SewingOrderRow({ order, onReceived }: {
  order: SewingOrderDto
  onReceived: (approvedUnits: number) => void
}) {
  const [open, setOpen]       = useState(false)
  const [approved, setApproved] = useState<Record<string, string>>(
    () => Object.fromEntries(order.items.map(i => [i.id, String(i.quantitySent)])))
  const [rejected, setRejected] = useState<Record<string, string>>(
    () => Object.fromEntries(order.items.map(i => [i.id, '0'])))
  const [busy, setBusy]   = useState(false)
  const [error, setError] = useState('')

  const sumsValid = order.items.every(i =>
    parseInt(approved[i.id] || '0') + parseInt(rejected[i.id] || '0') === i.quantitySent)

  const handleReceive = async () => {
    setError(''); setBusy(true)
    try {
      const res = await receiveSewingOrder(order.id, order.items.map(i => ({
        itemId: i.id,
        approved: parseInt(approved[i.id] || '0'),
        rejected: parseInt(rejected[i.id] || '0'),
      })))
      onReceived(res.totalApproved)
    } catch (err) { setError((err as Error).message) }
    finally { setBusy(false) }
  }

  return (
    <div className="rounded-lg overflow-hidden" style={{ border: '1px solid var(--border)' }}>
      <button onClick={() => setOpen(o => !o)}
        className="w-full flex items-center justify-between px-4 py-3 transition-colors hover:opacity-90"
        style={{ backgroundColor: 'var(--surface-2)' }}>
        <div className="flex items-center gap-3">
          <span className="font-mono font-bold text-sm accent">{order.orderCode}</span>
          <span className="text-xs muted">{order.confeccionistaName}</span>
          <StatusBadge status={order.status} />
        </div>
        <div className="flex items-center gap-3">
          <span className="text-xs subtle">
            {order.status === 'Assigned'
              ? `${num(order.totalSent)} u. enviadas`
              : `${num(order.totalApproved)} ✓ · ${num(order.totalRejected)} ✗`}
          </span>
          <span className="subtle text-sm">{open ? '▲' : '▼'}</span>
        </div>
      </button>

      {open && (
        <div className="p-4 space-y-3">
          <ErrorBox message={error} />
          <p className="text-xs subtle">Corte de origen: <span className="font-mono">{order.cuttingOrderCode}</span></p>
          <table className="w-full text-xs">
            <thead><tr className="table-head">
              {['SKU', 'Talla', 'Enviadas', 'Aprobadas', 'Rechazadas'].map(h => <th key={h}>{h}</th>)}
            </tr></thead>
            <tbody>
              {order.items.map(i => (
                <tr key={i.id} className="table-row">
                  <td className="px-3 py-1.5 font-mono accent">{i.sku}</td>
                  <td className="px-3 py-1.5">{i.size}</td>
                  <td className="px-3 py-1.5">{num(i.quantitySent)}</td>
                  {order.status === 'Assigned' ? (
                    <>
                      <td className="px-3 py-1.5">
                        <input type="number" min="0" max={i.quantitySent} value={approved[i.id] ?? ''}
                          onChange={e => setApproved(q => ({ ...q, [i.id]: e.target.value }))}
                          className="input w-20 py-1 px-2 text-xs" />
                      </td>
                      <td className="px-3 py-1.5">
                        <input type="number" min="0" max={i.quantitySent} value={rejected[i.id] ?? ''}
                          onChange={e => setRejected(q => ({ ...q, [i.id]: e.target.value }))}
                          className="input w-20 py-1 px-2 text-xs" />
                      </td>
                    </>
                  ) : (
                    <>
                      <td className="px-3 py-1.5" style={{ color: '#16a34a' }}>{num(i.quantityApproved)}</td>
                      <td className="px-3 py-1.5" style={{ color: '#dc2626' }}>{num(i.quantityRejected)}</td>
                    </>
                  )}
                </tr>
              ))}
            </tbody>
          </table>

          {order.status === 'Assigned' && (
            <>
              {!sumsValid && (
                <p className="text-xs" style={{ color: '#d97706' }}>
                  En cada ítem, aprobadas + rechazadas debe ser igual a las enviadas.
                </p>
              )}
              <button onClick={handleReceive} disabled={busy || !sumsValid} className="btn-primary w-full">
                {busy ? 'Recibiendo...' : 'Recibir con control de calidad'}
              </button>
            </>
          )}
        </div>
      )}
    </div>
  )
}

// ── Página ──────────────────────────────────────────────────────────────────
export default function ProduccionPage() {
  const [refs, setRefs]               = useState<ProductReferenceDto[]>([])
  const [selectedRef, setSelectedRef] = useState<ProductReferenceDto | null>(null)
  const [quantities, setQuantities]   = useState<Record<string, string>>({})
  const [notes, setNotes]             = useState('')
  const [talleres, setTalleres]       = useState<ConfeccionistaDto[]>([])
  const [cuttingOrders, setCuttingOrders] = useState<CuttingOrderDto[]>([])
  const [sewingOrders, setSewingOrders]   = useState<SewingOrderDto[]>([])
  const [submitting, setSubmitting]   = useState(false)
  const [error, setError]             = useState('')
  const [wmsNotice, setWmsNotice]     = useState('')

  const [showTallerForm, setShowTallerForm] = useState(false)
  const [tallerForm, setTallerForm] = useState({ name: '', contactName: '', phone: '', city: '' })
  const [tallerError, setTallerError] = useState('')

  const loadOrders = () => {
    getCuttingOrders().then(setCuttingOrders).catch(console.error)
    getSewingOrders().then(setSewingOrders).catch(console.error)
  }

  useEffect(() => {
    getProductReferences().then(r => { setRefs(r); if (r[0]) handleSelectRef(r[0]) }).catch(console.error)
    getConfeccionistas().then(setTalleres).catch(console.error)
    loadOrders()
  }, [])

  const handleSelectRef = (ref: ProductReferenceDto) => {
    setSelectedRef(ref)
    const q: Record<string, string> = {}
    ref.variants.forEach(v => { q[v.id] = '50' })
    setQuantities(q); setError('')
  }

  const handleCreateOrder = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRef) return
    setError(''); setSubmitting(true)
    try {
      await createCuttingOrder({
        productReferenceId: selectedRef.id,
        items: selectedRef.variants.filter(v => parseInt(quantities[v.id] || '0') > 0)
          .map(v => ({ variantId: v.id, quantity: parseInt(quantities[v.id]) })),
        notes: notes.trim() || null,
      })
      setNotes(''); loadOrders()
    } catch (err) { setError((err as Error).message) }
    finally { setSubmitting(false) }
  }

  const handleCreateTaller = async (e: React.FormEvent) => {
    e.preventDefault()
    setTallerError('')
    try {
      const t = await createConfeccionista(tallerForm)
      setTalleres(ts => [...ts, t])
      setTallerForm({ name: '', contactName: '', phone: '', city: '' })
      setShowTallerForm(false)
    } catch (err) { setTallerError((err as Error).message) }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>Producción</h1>
        <p>Corte interno → confección en talleres aliados → control de calidad</p>
      </div>

      {wmsNotice && (
        <div className="rounded-lg px-4 py-3 text-sm flex items-center justify-between"
          style={{ backgroundColor: 'rgba(22,163,74,0.1)', border: '1px solid rgba(22,163,74,0.3)', color: '#16a34a' }}>
          <span>{wmsNotice}</span>
          <button onClick={() => setWmsNotice('')} className="text-xs opacity-70 hover:opacity-100">✕</button>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Columna izquierda */}
        <div className="space-y-4">
          <form onSubmit={handleCreateOrder} className="card p-6 space-y-4">
            <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Nueva Orden de Corte</h2>
            <ErrorBox message={error} />

            <div>
              <label className="block text-xs font-medium mb-1 muted">Referencia</label>
              <select className="input" value={selectedRef?.id ?? ''}
                onChange={e => { const r = refs.find(x => x.id === e.target.value); if (r) handleSelectRef(r) }}>
                {refs.map(r => <option key={r.id} value={r.id}>{r.referenceCode} – {r.name}</option>)}
              </select>
            </div>

            {selectedRef && (
              <div>
                <label className="block text-xs font-medium mb-2 muted">Unidades a cortar por variante</label>
                <div className="space-y-2">
                  {selectedRef.variants.map(v => (
                    <div key={v.id} className="flex items-center gap-3">
                      <span className="w-3 h-3 rounded-full border shrink-0"
                        style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
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

            <div>
              <label className="block text-xs font-medium mb-1 muted">Notas (opcional)</label>
              <input className="input" value={notes} onChange={e => setNotes(e.target.value)}
                placeholder="Ej: tela satín referencia primavera" maxLength={500} />
            </div>

            <button type="submit" disabled={submitting || !selectedRef} className="btn-primary w-full">
              {submitting ? 'Creando...' : 'Crear Orden de Corte'}
            </button>
          </form>

          <div className="card overflow-hidden">
            <div className="card-header flex items-center justify-between">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Talleres de Confección</h2>
              <button onClick={() => setShowTallerForm(s => !s)} className="text-xs accent hover:underline">
                {showTallerForm ? 'Cancelar' : '+ Nuevo taller'}
              </button>
            </div>

            {showTallerForm && (
              <form onSubmit={handleCreateTaller} className="p-4 space-y-2" style={{ borderBottom: '1px solid var(--border)' }}>
                <ErrorBox message={tallerError} />
                <div className="grid grid-cols-2 gap-2">
                  <input className="input text-sm" placeholder="Nombre del taller" required
                    value={tallerForm.name} onChange={e => setTallerForm(f => ({ ...f, name: e.target.value }))} />
                  <input className="input text-sm" placeholder="Contacto"
                    value={tallerForm.contactName} onChange={e => setTallerForm(f => ({ ...f, contactName: e.target.value }))} />
                  <input className="input text-sm" placeholder="Teléfono" required
                    value={tallerForm.phone} onChange={e => setTallerForm(f => ({ ...f, phone: e.target.value }))} />
                  <input className="input text-sm" placeholder="Ciudad"
                    value={tallerForm.city} onChange={e => setTallerForm(f => ({ ...f, city: e.target.value }))} />
                </div>
                <button type="submit" className="btn-primary w-full">Guardar taller</button>
              </form>
            )}

            <table className="w-full text-xs">
              <thead><tr className="table-head">
                {['Taller', 'Contacto', 'Teléfono', 'Ciudad'].map(h => <th key={h}>{h}</th>)}
              </tr></thead>
              <tbody>
                {talleres.map(t => (
                  <tr key={t.id} className="table-row">
                    <td className="px-4 py-2 font-medium">{t.name}</td>
                    <td className="px-4 py-2 muted">{t.contactName}</td>
                    <td className="px-4 py-2 muted">{t.phone}</td>
                    <td className="px-4 py-2 muted">{t.city}</td>
                  </tr>
                ))}
                {talleres.length === 0 && (
                  <tr><td colSpan={4} className="px-4 py-6 text-center subtle">Sin talleres registrados</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Columna derecha */}
        <div className="space-y-4">
          <div className="card overflow-hidden">
            <div className="card-header">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Órdenes de Corte</h2>
            </div>
            <div className="p-4 space-y-2">
              {cuttingOrders.length === 0
                ? <div className="text-center subtle text-sm py-6">Sin órdenes de corte</div>
                : cuttingOrders.map(o => (
                    <CuttingOrderRow key={o.id} order={o} talleres={talleres} onChanged={loadOrders} />
                  ))}
            </div>
          </div>

          <div className="card overflow-hidden">
            <div className="card-header">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Órdenes de Confección</h2>
            </div>
            <div className="p-4 space-y-2">
              {sewingOrders.length === 0
                ? <div className="text-center subtle text-sm py-6">Sin órdenes de confección</div>
                : sewingOrders.map(o => (
                    <SewingOrderRow key={o.id} order={o}
                      onReceived={approvedUnits => {
                        loadOrders()
                        setWmsNotice(`${num(approvedUnits)} unidades aprobadas listas para ubicar en bodega (módulo WMS).`)
                      }} />
                  ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
