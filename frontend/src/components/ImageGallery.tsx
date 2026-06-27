'use client'
import { useState, useCallback, useEffect } from 'react'
import type { ProductImageDto } from '@/types/api'

interface Props {
  images: ProductImageDto[]
  canEdit?: boolean
  onUpload: (file: File) => Promise<void>
  onDelete: (imageId: string) => Promise<void>
}

export default function ImageGallery({ images, canEdit = false, onUpload, onDelete }: Props) {
  const [heroIdx, setHeroIdx]      = useState(0)
  const [uploading, setUploading]  = useState(false)
  const [dragOver, setDragOver]    = useState(false)
  const [deleting, setDeleting]    = useState<string | null>(null)
  const [lightbox, setLightbox]    = useState(false)

  // Resetea hero si el índice queda fuera de rango (p.ej. al eliminar la última foto)
  useEffect(() => {
    if (images.length > 0 && heroIdx >= images.length) setHeroIdx(images.length - 1)
  }, [images.length, heroIdx])

  const hero = images[Math.min(heroIdx, images.length - 1)]

  const handleFiles = useCallback(async (files: FileList | null) => {
    if (!files?.length) return
    setUploading(true)
    try { for (const f of Array.from(files)) await onUpload(f) }
    finally { setUploading(false) }
  }, [onUpload])

  const handleDelete = async (id: string, idx: number) => {
    setDeleting(id)
    try {
      await onDelete(id)
      if (idx <= heroIdx && heroIdx > 0) setHeroIdx(h => h - 1)
    } finally { setDeleting(null) }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault(); setDragOver(false)
    handleFiles(e.dataTransfer.files)
  }

  return (
    <div className="space-y-3">

      {/* ── Foto principal (hero) ── */}
      <div
        className="relative rounded-xl overflow-hidden flex items-center justify-center"
        style={{
          aspectRatio: '4/3',
          backgroundColor: 'var(--surface-2)',
          border: '1px solid var(--border)',
        }}
      >
        {hero ? (
          <>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={hero.url}
              alt={hero.originalName}
              className="w-full h-full object-contain cursor-zoom-in"
              onClick={() => setLightbox(true)}
            />
            {/* Botón eliminar hero */}
            {canEdit && (
              <button
                onClick={() => handleDelete(hero.id, heroIdx)}
                disabled={!!deleting}
                className="absolute top-2 right-2 w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold transition-opacity"
                style={{ backgroundColor: 'rgba(0,0,0,0.65)', color: '#fff' }}
                title="Eliminar foto"
              >
                ×
              </button>
            )}
            {/* Contador */}
            {images.length > 1 && (
              <span
                className="absolute bottom-2 right-2 text-xs px-1.5 py-0.5 rounded-full"
                style={{ backgroundColor: 'rgba(0,0,0,0.55)', color: '#fff' }}
              >
                {heroIdx + 1} / {images.length}
              </span>
            )}
            {/* Flechas de navegación */}
            {heroIdx > 0 && (
              <button
                onClick={() => setHeroIdx(i => i - 1)}
                className="absolute left-2 top-1/2 -translate-y-1/2 w-7 h-7 rounded-full flex items-center justify-center text-sm"
                style={{ backgroundColor: 'rgba(0,0,0,0.55)', color: '#fff' }}
              >‹</button>
            )}
            {heroIdx < images.length - 1 && (
              <button
                onClick={() => setHeroIdx(i => i + 1)}
                className="absolute right-2 top-1/2 -translate-y-1/2 w-7 h-7 rounded-full flex items-center justify-center text-sm"
                style={{ backgroundColor: 'rgba(0,0,0,0.55)', color: '#fff' }}
              >›</button>
            )}
          </>
        ) : (
          <div className="flex flex-col items-center gap-2 subtle">
            <span className="text-4xl">◻</span>
            <span className="text-xs">Sin fotos</span>
          </div>
        )}
      </div>

      {/* ── Tira de miniaturas ── */}
      {images.length > 1 && (
        <div className="flex gap-1.5 flex-wrap">
          {images.map((img, idx) => (
            <button
              key={img.id}
              onClick={() => setHeroIdx(idx)}
              className="relative group w-12 h-12 rounded-lg overflow-hidden flex-shrink-0 transition-all"
              style={{
                outline: idx === heroIdx ? '2px solid var(--accent)' : '1px solid var(--border)',
                outlineOffset: idx === heroIdx ? '1px' : '0px',
              }}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={img.url} alt="" className="w-full h-full object-cover" />
              {canEdit && (
                <span
                  onClick={e => { e.stopPropagation(); handleDelete(img.id, idx) }}
                  className="absolute inset-0 bg-black/50 text-white text-sm font-bold hidden group-hover:flex items-center justify-center"
                >
                  ×
                </span>
              )}
            </button>
          ))}
        </div>
      )}

      {/* ── Zona de subida ── */}
      {canEdit && (
        <label
          onDragOver={e => { e.preventDefault(); setDragOver(true) }}
          onDragLeave={() => setDragOver(false)}
          onDrop={handleDrop}
          className="flex items-center justify-center gap-2 py-3 rounded-lg cursor-pointer transition-colors"
          style={{
            border: `2px dashed ${dragOver ? 'var(--accent)' : 'var(--border)'}`,
            backgroundColor: dragOver ? 'var(--accent-light)' : 'transparent',
          }}
        >
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp"
            multiple
            className="hidden"
            onChange={e => handleFiles(e.target.files)}
            disabled={uploading}
          />
          {uploading ? (
            <span className="text-xs accent">Subiendo...</span>
          ) : (
            <>
              <span className="text-base" style={{ color: 'var(--accent)' }}>⊕</span>
              <span className="text-xs subtle">Arrastra o haz clic · JPG PNG WebP · 10 MB</span>
            </>
          )}
        </label>
      )}

      {/* ── Lightbox (pantalla completa al hacer clic en hero) ── */}
      {lightbox && hero && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center"
          style={{ backgroundColor: 'rgba(0,0,0,0.93)' }}
          onClick={() => setLightbox(false)}
        >
          <button
            className="absolute top-4 right-4 text-white text-2xl w-10 h-10 flex items-center justify-center rounded-full hover:bg-white/10"
            onClick={() => setLightbox(false)}
          >×</button>

          {heroIdx > 0 && (
            <button
              className="absolute left-4 text-white text-3xl w-12 h-12 flex items-center justify-center rounded-full hover:bg-white/10"
              onClick={e => { e.stopPropagation(); setHeroIdx(i => i - 1) }}
            >‹</button>
          )}

          <div className="max-w-4xl max-h-[90vh] px-16" onClick={e => e.stopPropagation()}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={hero.url}
              alt={hero.originalName}
              className="max-w-full max-h-[85vh] object-contain rounded-lg"
            />
            <p className="text-center text-xs mt-2" style={{ color: 'rgba(255,255,255,0.4)' }}>
              {hero.originalName} · {heroIdx + 1} / {images.length}
            </p>
          </div>

          {heroIdx < images.length - 1 && (
            <button
              className="absolute right-4 text-white text-3xl w-12 h-12 flex items-center justify-center rounded-full hover:bg-white/10"
              onClick={e => { e.stopPropagation(); setHeroIdx(i => i + 1) }}
            >›</button>
          )}
        </div>
      )}
    </div>
  )
}
