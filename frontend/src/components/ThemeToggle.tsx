'use client'
import { useTheme } from './ThemeProvider'

export default function ThemeToggle() {
  const { theme, toggle } = useTheme()
  return (
    <button
      onClick={toggle}
      title={theme === 'dark' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
      className="w-8 h-8 flex items-center justify-center rounded-lg
                 text-slate-400 hover:text-white hover:bg-slate-700
                 dark:text-slate-400 dark:hover:text-white dark:hover:bg-slate-700
                 transition-colors text-base"
    >
      {theme === 'dark' ? '☀' : '☽'}
    </button>
  )
}
