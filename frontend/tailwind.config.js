/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Web/**/*.{razor,html,cs}",
  ],
  theme: {
    extend: {
      colors: {
        // Paleta Biblioteca - Tons Quentes
        primary: {
          50: '#fdf8f3',
          100: '#fae8d6',
          200: '#f5d1ad',
          300: '#f0ba84',
          400: '#eba35b',
          500: '#e68a3c', // cor principal
          600: '#cc7131',
          700: '#b25826',
          800: '#984220',
          900: '#6d2d16',
        },
        secondary: {
          50: '#faf6f0',
          100: '#f1e4d0',
          200: '#e8d1af',
          300: '#dfbf8f',
          400: '#d6ad6e',
          500: '#c99a56', // destaque
          600: '#b88948',
          700: '#a6773a',
          800: '#94662d',
          900: '#6d4820',
        },
        accent: {
          50: '#f5f9f0',
          100: '#dfe8d0',
          200: '#c9d7b0',
          300: '#b2c690',
          400: '#9cb570',
          500: '#86a450', // verde biblioteca
          600: '#759044',
          700: '#647c38',
          800: '#53682c',
          900: '#3d4d20',
        },
        neutral: {
          50: '#faf9f7',
          100: '#f5f3f0',
          200: '#ebe5e0',
          300: '#e1d8d0',
          400: '#d7cbc0',
          500: '#cdbdb0',
          600: '#b8a899',
          700: '#9d8d7e',
          800: '#827569',
          900: '#5c4c42',
        },
      },
      fontFamily: {
        sans: ['Segoe UI', 'Roboto', 'Helvetica', 'Arial', 'sans-serif'],
        serif: ['Georgia', 'Garamond', 'serif'],
        mono: ['Fira Code', 'Courier New', 'monospace'],
      },
      fontSize: {
        xs: ['0.75rem', { lineHeight: '1rem' }],
        sm: ['0.875rem', { lineHeight: '1.25rem' }],
        base: ['1rem', { lineHeight: '1.5rem' }],
        lg: ['1.125rem', { lineHeight: '1.75rem' }],
        xl: ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
        '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
      },
      spacing: {
        '4xs': '0.25rem',
        '3xs': '0.5rem',
        '2xs': '0.75rem',
      },
      borderRadius: {
        xs: '0.25rem',
        sm: '0.375rem',
        base: '0.5rem',
        lg: '0.75rem',
        xl: '1rem',
      },
      boxShadow: {
        xs: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
        sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
        base: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
        md: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
        lg: '0 25px 50px -12px rgb(0 0 0 / 0.25)',
        xl: '0 20px 25px -5px rgba(234, 138, 60, 0.15)',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}
