'use client'
import { useEffect, useState } from 'react'
import { createProductReference, getProductReferences } from '@/lib/api/plm'
import { CATEGORY_OPTIONS } from '@/types/api'
import type { ProductReferenceDto } from '@/types/api'
import { cop, num } from '@/lib/utils/format'

interface VariantRow { sizeCode: string; colorName: string; colorHex: string; sku: string; costPrice: string }

const emptyVariant = (): VariantRow =>
  ({ sizeCode: '', colorName: '', colorHex: '#000000', sku: '', costPrice: '' })

export default function PlmPage() {
  const [refs, setRefs] = useState<ProductReferenceDto[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const [form, setForm] = useState({
    name: '', referenceCode: '', category: 1, description: '',
  })
  const [variants, setVariants] = useState<VariantRow[]>([emptyVariant()])

  const load = () => {
    setLoading(true)
    getProductReferences()
      .then(setRefs)
      .catch(() => setError('No se pudo conectar con el backend.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  const addVariant = () => setVariants(v => [...v, emptyVariant()])
  const removeVariant = (i: number) => setVariants(v => v.filter((_, j) => j !== i))
  const updateVariant = (i: number, field: keyof VariantRow, val: string) =>
    setVariants(v => v.map((row, j) => j === i ? { ...row, [field]: val } : row))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setSubmitting(true)
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
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="p-8 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">PLM — Ciclo de Vida del Producto</h1>
        <p className="text-sm text-slate-500 mt-1">Crea y gestiona referencias textiles con variaciones Talla × Color</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8">
        {/* Formulario */}
        <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 space-y-5">
          <h2 className="font-semibold text-slate-700">Nueva Referencia</h2>

          {error   && <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">{error}</div>}
          {success && <div className="rounded-lg bg-emerald-50 border border-emerald-200 px-4 py-3 text-sm text-emerald-700">{success}</div>}

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">Nombre</label>
              <input required value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
                placeholder="Pantalón Cargo" />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">Código</label>
              <input required value={form.referenceCode}
                onChange={e => setForm(f => ({ ...f, referenceCode: e.target.value }))}
                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
                placeholder="DYB-2024-001" />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-600 mb-1">Categoría</label>
            <select value={form.category}
              onChange={e => setForm(f => ({ ...f, category: Number(e.target.value) }))}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300">
              {CATEGORY_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-600 mb-1">Descripción</label>
            <textarea value={form.description} rows={2}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
              placeholder="Descripción del producto..." />
          </div>

          {/* Variantes */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs font-medium text-slate-600">Variantes (Talla × Color)</label>
              <button type="button" onClick={addVariant}
                className="text-xs text-indigo-600 hover:text-indigo-800 font-medium">+ Agregar</button>
            </div>
            <div className="space-y-2">
              {variants.map((v, i) => (
                <div key={i} className="grid grid-cols-5 gap-2 items-center">
                  <input placeholder="Talla (S/M/L)" value={v.sizeCode}
                    onChange={e => updateVariant(i, 'sizeCode', e.target.value)}
                    className="rounded-lg border border-slate-200 px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-indigo-300" />
                  <input placeholder="Color" value={v.colorName}
                    onChange={e => updateVariant(i, 'colorName', e.target.value)}
                    className="rounded-lg border border-slate-200 px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-indigo-300" />
                  <input type="color" value={v.colorHex}
                    onChange={e => updateVariant(i, 'colorHex', e.target.value)}
                    className="h-8 w-full rounded border border-slate-200 cursor-pointer" />
                  <input placeholder="SKU" value={v.sku}
                    onChange={e => updateVariant(i, 'sku', e.target.value)}
                    className="rounded-lg border border-slate-200 px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-indigo-300" />
                  <div className="flex gap-1">
                    <input placeholder="Costo" type="number" value={v.costPrice}
                      onChange={e => updateVariant(i, 'costPrice', e.target.value)}
                      className="rounded-lg border border-slate-200 px-2 py-1.5 text-xs w-full focus:outline-none focus:ring-1 focus:ring-indigo-300" />
                    {variants.length > 1 && (
                      <button type="button" onClick={() => removeVariant(i)}
                        className="text-red-400 hover:text-red-600 px-1 text-lg leading-none">×</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <button type="submit" disabled={submitting}
            className="w-full rounded-lg bg-indigo-600 text-white py-2.5 text-sm font-medium
                       hover:bg-indigo-700 disabled:opacity-50 transition-colors">
            {submitting ? 'Creando...' : 'Crear Referencia'}
          </button>
        </form>

        {/* Lista */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-slate-100">
            <h2 className="font-semibold text-slate-700">Referencias Activas</h2>
          </div>
          {loading ? (
            <div className="p-8 text-center text-slate-400 text-sm">Cargando...</div>
          ) : refs.length === 0 ? (
            <div className="p-8 text-center text-slate-400 text-sm">Sin referencias aún</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-slate-50 text-slate-500 text-xs uppercase">
                <tr>
                  {['Código', 'Nombre', 'Cat.', 'Vars.', 'Stock'].map(h => (
                    <th key={h} className="px-4 py-3 text-left font-medium">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {refs.map(r => (
                  <tr key={r.id} className="hover:bg-slate-50">
                    <td className="px-4 py-3 font-mono text-xs text-indigo-600">{r.referenceCode}</td>
                    <td className="px-4 py-3 font-medium text-slate-800">{r.name}</td>
                    <td className="px-4 py-3">
                      <span className="bg-indigo-100 text-indigo-700 rounded-full px-2 py-0.5 text-xs">{r.category}</span>
                    </td>
                    <td className="px-4 py-3 text-slate-600">{r.variants.length}</td>
                    <td className="px-4 py-3 font-medium">
                      {num(r.variants.reduce((a, v) => a + v.stockQuantity, 0))} u.
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  )
}
