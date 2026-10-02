/// <reference types="vitest/config" />
import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

const apiTarget = process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5080'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': apiTarget,
      '/hubs': { target: apiTarget, ws: true },
    },
  },
  test: {
    environment: 'jsdom',
  },
})
