<script setup lang="ts">
defineProps<{ title: string; subtitle?: string }>()
</script>

<template>
  <header class="page-header">
    <div>
      <h1>{{ title }}</h1>
      <p v-if="subtitle" class="subtitle">{{ subtitle }}</p>
    </div>
    <div class="actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<style scoped>
.page-header {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-4);
  margin-bottom: var(--space-5);
}
h1 {
  font-size: 1.5rem;
}
.subtitle {
  margin: var(--space-1) 0 0;
  color: var(--color-text-muted);
}
.actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}
</style>
