<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useToastStore } from '@/shared/stores/toasts'

const store = useToastStore()
const { toasts } = storeToRefs(store)
</script>

<template>
  <div class="toasts" aria-live="polite">
    <TransitionGroup name="toast">
      <div v-for="t in toasts" :key="t.id" class="toast" :class="t.tone" role="status">
        <span>{{ t.message }}</span>
        <button aria-label="Dismiss" @click="store.dismiss(t.id)">×</button>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toasts {
  position: fixed;
  right: var(--space-4);
  bottom: var(--space-4);
  display: grid;
  gap: var(--space-2);
  z-index: 60;
  max-width: min(380px, calc(100vw - 32px));
}
.toast {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-sm);
  border: 1px solid var(--color-border);
  border-left-width: 4px;
  background: var(--color-surface);
  box-shadow: 0 6px 20px rgb(0 0 0 / 18%);
}
.success {
  border-left-color: var(--color-success);
}
.danger {
  border-left-color: var(--color-danger);
}
.info {
  border-left-color: var(--color-info);
}
button {
  border: none;
  background: none;
  color: var(--color-text-muted);
  font-size: 1.1rem;
  cursor: pointer;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
.toast-enter-active,
.toast-leave-active {
  transition: all 0.2s ease;
}
</style>
