import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    // Must come BEFORE react() or autoCodeSplitting throws.
    tanstackRouter({ target: 'react', autoCodeSplitting: true }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    // Listen on all interfaces so the dev server is reachable when run in a container.
    host: true,
    watch: {
      // File-change events don't cross Docker bind mounts reliably on macOS/Windows,
      // so poll when running in the dev container (VITE_USE_POLLING=true).
      usePolling: process.env.VITE_USE_POLLING === 'true',
    },
  },
})
