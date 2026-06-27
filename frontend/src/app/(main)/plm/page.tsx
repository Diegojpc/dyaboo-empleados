'use client'
import { useEffect, useState, useCallback } from 'react'
import { createProductReference, getProductReferences, uploadProductImage, deleteProductImage } from '@/lib/api/plm'
import { CATEGORY_OPTIONS } from '@/types/api'
import type { ProductReferenceDto } from '@/types/api'
import { num } from '@/lib/utils/format'
import ImageGallery from '@/components/ImageGallery'

interface VariantRow { sizeCode: string; colorName: string; colorHex: string; sku: string; costPrice: string }
const emptyVariant = (): VariantRow => ({ sizeCode: '', colorName: '', colorHex: '#108474', sku: '', costPrice: '' })

export default function PlmPage() {
  const [refs, setRefs]             = useState<ProductReferenceDto[]>([])
  const [loading, setLoading]       = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError]           = useState('')
  const [success, setSuccess]       = useState('')
  const [form, setForm]             = useState({ name: '', referenceCode: '', category: 1, description: '' })
  const [variants, setVariants]     = useState<VariantRow[]>([emptyVariant()])
  const [selected, setSelected]     = useState<ProductReferenceDto | null>(null)

  const load = useCallback(() => {
    setLoading(true)
    getProductReferences()
      .then(data => {
        setRefs(data)
        setSelected(s => s ? (data.find(r => r.id === s.id) ?? null) : null)
      })
      .catch(() => setError('No se pudo conectar con el backend.'))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => { load() }, [load])

  const addVariant    = () => setVariants(v => [...v, emptyVariant()])
  const removeVariant = (i: number) => setVariants(v => v.filter((_, j) => j !== i))
  const updateVariant = (i: number, field: keyof VariantRow, val: string) =>
    setVariants(v => v.map((row, j) => j === i ? { ...row, [field]: val } : row))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(''); setSuccess(''); setSubmitting(true)
    try {
      await createProductReference({
        ...form,
        variants: variants.map(v => ({
          ...v,
          costPrice: parseFloat(v.costPrice),
          sizeCode: v.sizeCode.toUpperCase(),
        })),
      })
      setSuccess('Referencia creada exitosamente.')
      setForm({ name: '', referenceCode: '', category: 1, description: '' })
      setVariants([emptyVariant()])
      load()
    } catch (err) { setError((err as Error).message) }
    finally { setSubmitting(false) }
  }

  const handleUpload = async (file: File) => {
    if (!selected) return
    await uploadProductImage(selected.id, file)
    load()
  }

  const handleDelete = async (imageId: string) => {
    if (!selected) return
    await deleteProductImage(selected.id, imageId)
    load()
  }

  return (
    <div className="p-6 space-y-6">
      <div className="page-header">
        <h1>PLM — Ciclo de Vida del Producto</h1>
        <p>Crea y gestiona referencias textiles con variaciones Talla × Color</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* ── Formulario nueva referencia ── */}
        <form onSubmit={handleSubmit} className="card p-5 space-y-4">
          <h2 className="text-sm font-semibold" style={{ color: 'var(--text)' }}>Nueva Referencia</h2>

          {error   && <div className="rounded-lg px-3 py-2 text-xs" style={{ backgroundColor: 'rgba(139,0,0,0.1)', border: '1px solid rgba(139,0,0,0.3)', color: '#dc2626' }}>{error}</div>}
          {success && <div className="rounded-lg px-3 py-2 text-xs" style={{ backgroundColor: 'var(--accent-light)', border: '1px solid var(--accent)', color: 'var(--accent)' }}>{success}</div>}

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium mb-1 muted">Nombre</label>
              <input required value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                className="input" placeholder="Pantalón Cargo" />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1 muted">Código</label>
              <input required value={form.referenceCode} onChange={e => setForm(f => ({ ...f, referenceCode: e.target.value }))}
                className="input" placeholder="DYB-001" />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium mb-1 muted">Categoría</label>
            <select value={form.category} onChange={e => setForm(f => ({ ...f, category: Number(e.target.value) }))} className="input">
              {CATEGORY_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium mb-1 muted">Descripción</label>
            <textarea value={form.description} rows={2} onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              className="input" placeholder="Descripción del producto..." />
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs font-medium muted">Variantes</label>
              <button type="button" onClick={addVariant} className="text-xs font-medium accent hover:opacity-80">+ Agregar</button>
            </div>
            <div className="space-y-1.5">
              {variants.map((v, i) => (
                <div key={i} className="grid grid-cols-5 gap-1.5 items-center">
                  <input placeholder="Talla" value={v.sizeCode} onChange={e => updateVariant(i, 'sizeCode', e.target.value)}
                    className="input text-xs py-1 px-2" />
                  <input placeholder="Color" value={v.colorName} onChange={e => updateVariant(i, 'colorName', e.target.value)}
                    className="input text-xs py-1 px-2" />
                  <input type="color" value={v.colorHex} onChange={e => updateVariant(i, 'colorHex', e.target.value)}
                    className="h-7 w-full rounded cursor-pointer border" style={{ borderColor: 'var(--border)', backgroundColor: 'var(--surface)' }} />
                  <input placeholder="SKU" value={v.sku} onChange={e => updateVariant(i, 'sku', e.target.value)}
                    className="input text-xs py-1 px-2" />
                  <div className="flex gap-1">
                    <input placeholder="$" type="number" value={v.costPrice} onChange={e => updateVariant(i, 'costPrice', e.target.value)}
                      className="input text-xs py-1 px-2 w-full" />
                    {variants.length > 1 && (
                      <button type="button" onClick={() => removeVariant(i)} className="text-red-400 hover:text-red-600 px-1 text-base leading-none">×</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <button type="submit" disabled={submitting} className="btn-primary w-full text-xs py-2">
            {submitting ? 'Creando...' : 'Crear Referencia'}
          </button>
        </form>

        {/* ── Lista de referencias ── */}
        <div className="card overflow-hidden xl:col-span-2 flex flex-col">
          <div className="card-header py-3">
            <h2 className="text-sm font-semibold" style={{ color: 'var(--text)' }}>Referencias Activas</h2>
          </div>
          <div className="overflow-x-auto flex-1">
            {loading ? (
              <div className="p-8 text-center subtle text-sm">Cargando...</div>
            ) : refs.length === 0 ? (
              <div className="p-8 text-center subtle text-sm">Sin referencias aún</div>
            ) : (
              <table className="w-full">
                <thead>
                  <tr className="table-head">
                    <th>Foto</th>
                    <th>Código</th>
                    <th>Nombre</th>
                    <th>Cat.</th>
                    <th>Vars.</th>
                    <th>Stock</th>
                    <th>Fotos</th>
                  </tr>
                </thead>
                <tbody>
                  {refs.map(r => {
                    const isActive = selected?.id === r.id
                    return (
                      <tr
                        key={r.id}
                        className="table-row cursor-pointer"
                        style={isActive ? { backgroundColor: 'var(--accent-light)' } : {}}
                        onClick={() => setSelected(isActive ? null : r)}
                      >
                        <td className="px-3 py-2 w-10">
                          {r.images[0] ? (
                            // eslint-disable-next-line @next/next/no-img-element
                            <img
                              src={r.images[0].url}
                              alt=""
                              className="w-8 h-8 rounded object-cover"
                              style={{ border: '1px solid var(--border)' }}
                            />
                          ) : (
                            <div className="w-8 h-8 rounded flex items-center justify-center text-xs subtle"
                              style={{ border: '1px dashed var(--border)', backgroundColor: 'var(--surface-2)' }}>
                              ◻
                            </div>
                          )}
                        </td>
                        <td className="px-3 py-2 font-mono text-xs accent">{r.referenceCode}</td>
                        <td className="px-3 py-2 text-xs font-medium">{r.name}</td>
                        <td className="px-3 py-2"><span className="badge">{r.category}</span></td>
                        <td className="px-3 py-2 text-xs muted">{r.variants.length}</td>
                        <td className="px-3 py-2 text-xs font-medium">
                          {num(r.variants.reduce((a, v) => a + v.stockQuantity, 0))} u.
                        </td>
                        <td className="px-3 py-2 text-xs muted">{r.images.length}</td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>

      {/* ── Panel de galería ── */}
      {selected && (
        <div className="card p-5">
          <div className="flex items-start justify-between mb-4">
            <div>
              <h2 className="text-sm font-semibold" style={{ color: 'var(--text)' }}>
                {selected.name}
              </h2>
              <p className="text-xs muted">{selected.referenceCode} · {selected.category} · {selected.images.length} foto{selected.images.length !== 1 ? 's' : ''}</p>
              {selected.description && <p className="text-xs subtle mt-1">{selected.description}</p>}
            </div>
            <button onClick={() => setSelected(null)} className="text-xs subtle hover:muted px-2 py-1 rounded"
              style={{ border: '1px solid var(--border)' }}>
              Cerrar
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <p className="text-xs font-medium muted mb-2 uppercase tracking-wider">Galería de fotos</p>
              <ImageGallery
                images={selected.images}
                canEdit
                onUpload={handleUpload}
                onDelete={handleDelete}
              />
            </div>

            <div>
              <p className="text-xs font-medium muted mb-2 uppercase tracking-wider">Variantes</p>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="table-head">
                      <th>SKU</th>
                      <th>Talla</th>
                      <th>Color</th>
                      <th>Costo</th>
                      <th>Stock</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selected.variants.map(v => (
                      <tr key={v.id} className="table-row">
                        <td className="px-3 py-1.5 font-mono text-xs accent">{v.sku}</td>
                        <td className="px-3 py-1.5 text-xs">{v.sizeCode}</td>
                        <td className="px-3 py-1.5 text-xs">
                          <div className="flex items-center gap-1.5">
                            <span className="w-3 h-3 rounded-full shrink-0 border" style={{ backgroundColor: v.colorHex, borderColor: 'var(--border)' }} />
                            {v.colorName}
                          </div>
                        </td>
                        <td className="px-3 py-1.5 text-xs muted">${v.costPrice.toLocaleString()}</td>
                        <td className="px-3 py-1.5 text-xs font-medium">{num(v.stockQuantity)} u.</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
