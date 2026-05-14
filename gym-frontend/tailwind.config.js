/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],

  darkMode: 'class',

  theme: {
    extend: {
      colors: {
        primary: {
          50: '#fff8f2',
          100: '#ffeddc',
          200: '#ffd0ad',
          300: '#ffb37d',
          400: '#ff9147',
          500: '#FF6C13',
          600: '#e85d04',
          700: '#c94c00',
          800: '#9f3c00',
          900: '#7e3000',
          950: '#441700',
        },

        dark: {
          50: '#f5f5f5',
          100: '#e5e5e5',
          200: '#d4d4d4',
          300: '#a3a3a3',
          400: '#737373',
          500: '#525252',
          600: '#404040',
          700: '#262626',
          800: '#171717',
          900: '#0f0f0f',
          950: '#080808',
        },

        success: {
          500: '#22c55e',
        },

        danger: {
          500: '#ef4444',
        },
      },

      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },

      borderRadius: {
        xl: '1rem',
        '2xl': '1.5rem',
      },

      boxShadow: {
        glow: '0 0 20px rgba(255,108,19,0.25)',
        'glow-lg': '0 0 40px rgba(255,108,19,0.35)',

        soft: '0 4px 20px rgba(0,0,0,0.25)',
        card: '0 10px 30px rgba(0,0,0,0.35)',
      },

      transitionTimingFunction: {
        smooth: 'cubic-bezier(0.4, 0, 0.2, 1)',
      },

      animation: {
        'fade-in': 'fadeIn 0.4s ease-out',
        'slide-up': 'slideUp 0.4s cubic-bezier(0.4,0,0.2,1)',
        'slide-in-right': 'slideInRight 0.4s cubic-bezier(0.4,0,0.2,1)',
      },

      keyframes: {
        fadeIn: {
          '0%': {
            opacity: '0',
          },
          '100%': {
            opacity: '1',
          },
        },

        slideUp: {
          '0%': {
            opacity: '0',
            transform: 'translateY(12px)',
          },
          '100%': {
            opacity: '1',
            transform: 'translateY(0)',
          },
        },

        slideInRight: {
          '0%': {
            opacity: '0',
            transform: 'translateX(24px)',
          },
          '100%': {
            opacity: '1',
            transform: 'translateX(0)',
          },
        },
      },
    },
  },

  plugins: [],
}