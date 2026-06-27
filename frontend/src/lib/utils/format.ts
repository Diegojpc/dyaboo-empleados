export const cop = (n: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(n)

export const pct = (n: number) => `${n.toFixed(2)}%`

export const num = (n: number) =>
  new Intl.NumberFormat('es-CO').format(n)
