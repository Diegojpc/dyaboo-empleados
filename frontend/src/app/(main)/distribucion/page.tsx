'use client'
import { useEffect, useState } from 'react'
import {
  getCustomers, createCustomer,
  getSalesOrders, createSalesOrder,
  confirmSalesOrder, dispatchSalesOrder, deliverSalesOrder, cancelSalesOrder,
} from '@/lib/api/distribucion'
import { getProductReferences } from '@/lib/api/plm'
import type {
  ProductReferenceDto, CustomerDto, SalesOrderDto, DispatchResult, SalesOrderStatus,
} from '@/types/api'
import { num, cop } from '@/lib/utils/format'

const CUSTOMER_TYPE_LABEL: Record<string, string> = {
  TiendaPropia: 'Tienda propia',
  MayoristaExterno: 'Mayorista',
}

function StatusBadge({ status }: { status: SalesOrderStatus }) {
  const styles: Record<SalesOrderStatus, { bg: string; fg: string; label: string }> = {
    Draft:      { bg: 'var(--surface-2)',       fg: 'var(--text)', label: 'Borrador' },
    Confirmed:  { bg: 'rgba(37,99,235,0.12)',   fg: '#2563eb',     label: 'Confirmado' },
    Dispatched: { bg: 'rgba(217,119,6,0.12)',   fg: '#d97706',     label: 'Despachado' },
    Delivered:  { bg: 'rgba(22,163,74,0.12)',   fg: '#16a34a',     label: 'Entregado' },
    Cancelled:  { bg: 'rgba(220,38,38,0.12)',   fg: '#dc2626',     label: 'Cancelado' },
  }
  const s = styles[status]
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

// ── Panel por pedido ────────────────────────────────────────────────────────
function SalesOrderRow({ order, onChanged, onDispatched }: {
  order: SalesOrderDto
  onChanged: () => void
  onDispatched: (result: DispatchResult) => void
}) {
  const [open, setOpen]   = useState(false)
  const [busy, setBusy]   = useState(false)
  const [error, setError] = useState('')

  const run = async (fn: () => Promise<unknown>) => {
    setError(''); setBusy(true)
    try { await fn(); onChanged() }
    catch (err) { setError((err as Error).message) }
    finally { setBusy(false) }
  }

  const handleDispatch = async () => {
    setError(''); setBusy(true)
    try {
      const res = await dispatchSalesOrder(order.id)
      onDispatched(res); onChanged()
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
          <span className="text-xs muted">{order.customerName}</span>
          <StatusBadge status={order.status} />
        </div>
        <div className="flex items-center gap-3">
          <span className="text-xs subtle">{num(order.totalUnits)} u. · {cop(order.total)}</span>
          <span className="subtle text-sm">{open ? '▲' : '▼'}</span>
        </div>
      </button>

      {open && (
        <div className="p-4 space-y-3">
          <ErrorBox message={error} />
          <table className="w-full text-xs">
            <thead><tr className="table-head">
              {['SKU', 'Talla', 'Color', 'Cantidad', 'Precio u.', 'Subtotal'].map(h => <th key={h}>{h}</th>)}
            </tr></thead>
            <tbody>
              {order.items.map(i => (
                <tr key={i.id} className="table-row">
                  <td className="px-3 py-1.5 font-mono accent">{i.sku}</td>
                  <td className="px-3 py-1.5">{i.size}</td>
                  <td className="px-3 py-1.5">{i.color}</td>
                  <td className="px-3 py-1.5">{num(i.quantity)}</td>
                  <td className="px-3 py-1.5">{cop(i.unitPrice)}</td>
                  <td className="px-3 py-1.5 font-medium">{cop(i.lineTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          {order.notes && <p className="text-xs subtle">Notas: {order.notes}</p>}

          <div className="flex items-center gap-2 flex-wrap">
            {order.status === 'Draft' && (
              <button onClick={() => run(() => confirmSalesOrder(order.id))} disabled={busy} className="btn-primary">
                Confirmar
              </button>
            )}
            {order.status === 'Confirmed' && (
              <button onClick={handleDispatch} disabled={busy} className="btn-primary">
                {busy ? 'Despachando...' : 'Despachar'}
              </button>
            )}
            {order.status === 'Dispatched' && (
              <button onClick={() => run(() => deliverSalesOrder(order.id))} disabled={busy} className="btn-primary">
                Marcar entregado
              </button>
            )}
            {(order.status === 'Draft' || order.status === 'Confirmed') && (
              <button onClick={() => run(() => cancelSalesOrder(order.id))} disabled={busy}
                className="text-xs px-3 py-2 rounded-lg transition-colors hover:opacity-80"
                style={{ border: '1px solid rgba(220,38,38,0.4)', color: '#dc2626' }}>
                Cancelar pedido
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

// ── Página ──────────────────────────────────────────────────────────────────
export default function DistribucionPage() {
  const [customers, setCustomers]   = useState<CustomerDto[]>([])
  const [refs, setRefs]             = useState<ProductReferenceDto[]>([])
  const [orders, setOrders]         = useState<SalesOrderDto[]>([])
  const [customerId, setCustomerId] = useState('')
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const [prices, setPrices]         = useState<Record<string, string>>({})
  const [notes, setNotes]           = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError]           = useState('')
  const [picking, setPicking]       = useState<DispatchResult | null>(null)

  const [showCustomerForm, setShowCustomerForm] = useState(false)
  const [customerForm, setCustomerForm] = useState({ name: '', type: 1, contactName: '', phone: '', city: '' })
  const [customerError, setCustomerError] = useState('')

  const allVariants = refs.flatMap(r =>
    r.variants.map(v => ({ ...v, refName: r.name, refCode: r.referenceCode })))

  const loadOrders = () => getSalesOrders().then(setOrders).catch(console.error)

  useEffect(() => {
    getCustomers().then(c => { setCustomers(c); if (c[0]) setCustomerId(c[0].id) }).catch(console.error)
    getProductReferences().then(setRefs).catch(console.error)
    loadOrders()
  }, [])

  const orderTotal = allVariants.reduce((acc, v) => {
    const q = parseInt(quantities[v.id] || '0')
    const p = parseFloat(prices[v.id] || '0')
    return acc + (q > 0 && p > 0 ? q * p : 0)
  }, 0)

  const handleCreateOrder = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!customerId) return
    const items = allVariants
      .filter(v => parseInt(quantities[v.id] || '0') > 0)
      .map(v => ({
        variantId: v.id,
        quantity: parseInt(quantities[v.id]),
        unitPrice: parseFloat(prices[v.id] || '0'),
      }))
    if (items.length === 0) { setError('Agrega al menos una variante con cantidad.'); return }
    setError(''); setSubmitting(true)
    try {
      await createSalesOrder({ customerId, items, notes: notes.trim() || null })
      setQuantities({}); setPrices({}); setNotes(''); loadOrders()
    } catch (err) { setError((err as Error).message) }
    finally { setSubmitting(false) }
  }

  const handleCreateCustomer = async (e: React.FormEvent) => {
    e.preventDefault()
    setCustomerError('')
    try {
      const c = await createCustomer(customerForm)
      setCustomers(cs => [...cs, c])
      setCustomerForm({ name: '', type: 1, contactName: '', phone: '', city: '' })
      setShowCustomerForm(false)
    } catch (err) { setCustomerError((err as Error).message) }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>Distribución</h1>
        <p>Pedidos de tiendas propias y mayoristas con despacho desde bodega</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Columna izquierda */}
        <div className="space-y-4">
          <form onSubmit={handleCreateOrder} className="card p-6 space-y-4">
            <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Nuevo Pedido</h2>
            <ErrorBox message={error} />

            <div>
              <label className="block text-xs font-medium mb-1 muted">Cliente</label>
              <select className="input" value={customerId} onChange={e => setCustomerId(e.target.value)}>
                {customers.map(c => (
                  <option key={c.id} value={c.id}>
                    {c.name} ({CUSTOMER_TYPE_LABEL[c.type] ?? c.type})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium mb-2 muted">Variantes (stock disponible)</label>
              <div className="space-y-2 max-h-72 overflow-y-auto pr-1">
                {allVariants.map(v => (
                  <div key={v.id} className="flex items-center gap-2">
                    <span className="w-3 h-3 rounded-full border shrink-0"
                      style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
                    <span className="font-mono text-xs accent w-28 shrink-0 truncate" title={v.sku}>{v.sku}</span>
                    <span className="text-xs muted w-8 shrink-0">{v.sizeCode}</span>
                    <span className="text-xs subtle w-14 shrink-0">{num(v.stockQuantity)} u.</span>
                    <input type="number" min="0" placeholder="Cant." value={quantities[v.id] ?? ''}
                      onChange={e => setQuantities(q => ({ ...q, [v.id]: e.target.value }))}
                      className="input w-20 py-1 px-2 text-xs" />
                    <input type="number" min="0" step="100" placeholder="Precio u." value={prices[v.id] ?? ''}
                      onChange={e => setPrices(p => ({ ...p, [v.id]: e.target.value }))}
                      className="input w-28 py-1 px-2 text-xs" />
                  </div>
                ))}
                {allVariants.length === 0 && (
                  <p className="text-xs subtle py-4 text-center">Sin referencias en el PLM</p>
                )}
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium mb-1 muted">Notas (opcional)</label>
              <input className="input" value={notes} onChange={e => setNotes(e.target.value)}
                placeholder="Ej: entregar antes del viernes" maxLength={500} />
            </div>

            <div className="flex items-center justify-between">
              <span className="text-xs muted">Total del pedido</span>
              <span className="font-semibold accent">{cop(orderTotal)}</span>
            </div>

            <button type="submit" disabled={submitting || !customerId} className="btn-primary w-full">
              {submitting ? 'Creando...' : 'Crear Pedido'}
            </button>
          </form>

          <div className="card overflow-hidden">
            <div className="card-header flex items-center justify-between">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Clientes</h2>
              <button onClick={() => setShowCustomerForm(s => !s)} className="text-xs accent hover:underline">
                {showCustomerForm ? 'Cancelar' : '+ Nuevo cliente'}
              </button>
            </div>

            {showCustomerForm && (
              <form onSubmit={handleCreateCustomer} className="p-4 space-y-2" style={{ borderBottom: '1px solid var(--border)' }}>
                <ErrorBox message={customerError} />
                <div className="grid grid-cols-2 gap-2">
                  <input className="input text-sm" placeholder="Nombre" required
                    value={customerForm.name} onChange={e => setCustomerForm(f => ({ ...f, name: e.target.value }))} />
                  <select className="input text-sm" value={customerForm.type}
                    onChange={e => setCustomerForm(f => ({ ...f, type: parseInt(e.target.value) }))}>
                    <option value={1}>Tienda propia</option>
                    <option value={2}>Mayorista externo</option>
                  </select>
                  <input className="input text-sm" placeholder="Contacto"
                    value={customerForm.contactName} onChange={e => setCustomerForm(f => ({ ...f, contactName: e.target.value }))} />
                  <input className="input text-sm" placeholder="Teléfono" required
                    value={customerForm.phone} onChange={e => setCustomerForm(f => ({ ...f, phone: e.target.value }))} />
                  <input className="input text-sm col-span-2" placeholder="Ciudad"
                    value={customerForm.city} onChange={e => setCustomerForm(f => ({ ...f, city: e.target.value }))} />
                </div>
                <button type="submit" className="btn-primary w-full">Guardar cliente</button>
              </form>
            )}

            <table className="w-full text-xs">
              <thead><tr className="table-head">
                {['Cliente', 'Tipo', 'Contacto', 'Ciudad'].map(h => <th key={h}>{h}</th>)}
              </tr></thead>
              <tbody>
                {customers.map(c => (
                  <tr key={c.id} className="table-row">
                    <td className="px-4 py-2 font-medium">{c.name}</td>
                    <td className="px-4 py-2 muted">{CUSTOMER_TYPE_LABEL[c.type] ?? c.type}</td>
                    <td className="px-4 py-2 muted">{c.contactName}</td>
                    <td className="px-4 py-2 muted">{c.city}</td>
                  </tr>
                ))}
                {customers.length === 0 && (
                  <tr><td colSpan={4} className="px-4 py-6 text-center subtle">Sin clientes registrados</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Columna derecha */}
        <div className="space-y-4">
          {picking && (
            <div className="card p-6 space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold" style={{ color: 'var(--text)' }}>
                  Lista de picking — {picking.orderCode}
                </h3>
                <div className="flex items-center gap-3">
                  <span className="text-xs subtle">{num(picking.totalUnitsDispatched)} u. despachadas</span>
                  <button onClick={() => setPicking(null)} className="text-xs subtle hover:opacity-70">✕</button>
                </div>
              </div>
              <table className="w-full text-xs">
                <thead><tr className="table-head">
                  {['Ubicación', 'SKU', 'Cantidad'].map(h => <th key={h}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {picking.pickingLines.map((l, i) => (
                    <tr key={i} className="table-row">
                      <td className="px-3 py-1.5 font-mono font-bold accent">{l.locationCode}</td>
                      <td className="px-3 py-1.5 font-mono accent">{l.sku}</td>
                      <td className="px-3 py-1.5 font-medium">{num(l.quantity)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <div className="card overflow-hidden">
            <div className="card-header">
              <h2 className="font-semibold" style={{ color: 'var(--text)' }}>Pedidos</h2>
            </div>
            <div className="p-4 space-y-2">
              {orders.length === 0
                ? <div className="text-center subtle text-sm py-6">Sin pedidos</div>
                : orders.map(o => (
                    <SalesOrderRow key={o.id} order={o} onChanged={loadOrders} onDispatched={setPicking} />
                  ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
