<script setup lang="ts">
withDefaults(defineProps<{ variant?: 'primary' | 'secondary' | 'ghost' | 'danger'; size?: 'sm' | 'md'; loading?: boolean }>(), {
  variant: 'secondary',
  size: 'md',
  loading: false,
})
</script>

<template>
  <button class="btn" :class="[variant, size]" :disabled="loading || ($attrs.disabled as boolean)" :aria-busy="loading">
    <span v-if="loading" class="spinner" aria-hidden="true" />
    <slot />
  </button>
</template>

<style scoped>
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-text);
  font-weight: 550;
  cursor: pointer;
  white-space: nowrap;
}
.md {
  padding: 7px 14px;
}
.sm {
  padding: 3px 10px;
  font-size: 12px;
}
.btn:hover:not(:disabled) {
  background: var(--color-surface-hover);
}
.btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--color-accent);
  outline-offset: 2px;
}
.primary {
  background: var(--color-accent);
  border-color: var(--color-accent);
  color: #fff;
}
.primary:hover:not(:disabled) {
  background: var(--color-accent);
  filter: brightness(1.08);
}
.ghost {
  border-color: transparent;
  background: transparent;
}
.danger {
  color: var(--color-danger);
}
.spinner {
  width: 12px;
  height: 12px;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
