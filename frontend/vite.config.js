import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// Vue dev server runs on http://localhost:5173 and calls the API at http://localhost:5038.
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173
  }
})
