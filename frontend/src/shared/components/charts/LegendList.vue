<script setup lang="ts">
defineProps<{ items: { label: string; color: string }[]; shape?: 'rect' | 'line' }>()
</script>

<template>
  <ul class="legend">
    <li v-for="item in items" :key="item.label">
      <span class="key" :class="shape ?? 'rect'" :style="{ background: item.color }" aria-hidden="true" />
      {{ item.label }}
    </li>
  </ul>
</template>

<style scoped>
.legend {
  list-style: none;
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2) var(--space-4);
  margin: 0 0 var(--space-3);
  padding: 0;
  font-size: 12px;
  color: var(--color-text-muted);
}
li {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
.key.rect {
  width: 10px;
  height: 10px;
  border-radius: 2px;
}
.key.line {
  width: 14px;
  height: 2px;
  border-radius: 1px;
}
</style>
