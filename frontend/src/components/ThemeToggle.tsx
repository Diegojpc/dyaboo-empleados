'use client'
import { useTheme } from './ThemeProvider'

export default function ThemeToggle() {
  const { theme, toggle } = useTheme()
  const isDark = theme === 'dark'

  return (
    <button
      onClick={toggle}
      title={isDark ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
      className="relative flex items-center w-11 h-6 rounded-full transition-colors duration-200 focus:outline-none"
      style={{ backgroundColor: isDark ? 'var(--accent)' : '#e5d3c4' }}
    >
      <span
        className="absolute w-5 h-5 bg-white rounded-full shadow transition-transform duration-200 flex items-center justify-center text-[10px]"
        style={{ transform: isDark ? 'translateX(22px)' : 'translateX(2px)' }}
      >
        {isDark ? '☽' : '☀'}
      </span>
    </button>
  )
}
