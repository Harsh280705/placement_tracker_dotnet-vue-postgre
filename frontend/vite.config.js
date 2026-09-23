import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// Vue dev server runs on http://localhost:5173.
// The frontend calls the API via the relative path /api/... which works both
// directly (:5173, proxied below) and through the NGINX entry point (:8080).
export default defineConfig({
  plugins: [vue()],
  server: {
    host: '127.0.0.1',
    port: 5173,

    allowedHosts: [
      'pedigree-silicon-levitate.ngrok-free.dev'
    ],

    proxy: {
      '/api': {
        target: 'http://localhost:5038',
        changeOrigin: true
      }
    }
  }
})