<script setup lang="ts">
import { ref } from 'vue'
import Card from '@/shared/components/Card.vue'

defineProps<{ title: string; subtitle?: string; loading?: boolean }>()
const view = ref<'chart' | 'table'>('chart')
</script>

<template>
  <Card :title="title" :subtitle="subtitle">
    <template #actions>
      <div class="toggle" role="tablist" aria-label="View">
        <button role="tab" :aria-selected="view === 'chart'" :class="{ active: view === 'chart' }" @click="view = 'chart'">Chart</button>
        <button role="tab" :aria-selected="view === 'table'" :class="{ active: view === 'table' }" @click="view = 'table'">Table</button>
      </div>
    </template>
    <!-- Keep the previous render while refetching, at reduced opacity, instead of flashing. -->
    <div :class="{ refreshing: loading }">
      <slot v-if="view === 'chart'" />
      <div v-else class="table-wrap"><slot name="table" /></div>
    </div>
  </Card>
</template>

<style scoped>
.toggle {
  display: inline-flex;
  padding: 2px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
}
.toggle button {
  border: none;
  background: none;
  padding: 2px 10px;
  border-radius: 4px;
  font-size: 12px;
  color: var(--color-text-muted);
  cursor: pointer;
}
.toggle button.active {
  background: var(--color-surface-hover);
  color: var(--color-text);
  font-weight: 600;
}
.refreshing {
  opacity: 0.6;
  transition: opacity 0.2s;
}
.table-wrap {
  overflow-x: auto;
  max-height: 340px;
  overflow-y: auto;
}
.table-wrap :deep(table) {
  width: 100%;
  border-collapse: collapse;
  white-space: nowrap;
}
.table-wrap :deep(th) {
  position: sticky;
  top: 0;
  background: var(--color-surface);
  text-align: left;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
.table-wrap :deep(td) {
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
.table-wrap :deep(.num) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>
