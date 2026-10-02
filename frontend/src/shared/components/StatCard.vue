<script setup lang="ts">
defineProps<{ label: string; value: string; hint?: string; tone?: 'default' | 'success' | 'danger' | 'info' }>()
</script>

<template>
  <div class="stat" :class="tone ?? 'default'">
    <div class="label">{{ label }}</div>
    <div class="value tabular">{{ value }}</div>
    <div v-if="hint" class="hint">{{ hint }}</div>
  </div>
</template>

<style scoped>
.stat {
  padding: var(--space-4);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  box-shadow: var(--shadow-sm);
  min-width: 0;
}
.label {
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 500;
}
.value {
  margin-top: var(--space-1);
  font-size: 1.6rem;
  font-weight: 650;
  overflow: hidden;
  text-overflow: ellipsis;
}
.hint {
  margin-top: 2px;
  color: var(--color-text-muted);
  font-size: 12px;
}
.success .value {
  color: var(--color-success);
}
.danger .value {
  color: var(--color-danger);
}
.info .value {
  color: var(--color-info);
}
</style>
