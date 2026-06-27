import type { Config } from 'tailwindcss'

const config: Config = {
  content: ['./src/**/*.{js,ts,jsx,tsx,mdx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        dyaboo: {
          teal:         '#108474',
          'teal-hover': '#0a6359',
          'teal-light': '#e8f5f3',
          cream:        '#F0EFEB',
          beige:        '#eee6de',
          tan:          '#e5d3c4',
          brown:        '#473c38',
        },
      },
    },
  },
  plugins: [],
}

export default config
