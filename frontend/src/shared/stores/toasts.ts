import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface Toast {
  id: number
  message: string
  tone: 'success' | 'danger' | 'info'
}

export const useToastStore = defineStore('toasts', () => {
  const toasts = ref<Toast[]>([])
  let nextId = 1

  function dismiss(id: number) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  function push(message: string, tone: Toast['tone'] = 'info', timeoutMs = 4000) {
    const id = nextId++
    toasts.value = [...toasts.value, { id, message, tone }]
    setTimeout(() => dismiss(id), timeoutMs)
  }

  return { toasts, push, dismiss }
})
