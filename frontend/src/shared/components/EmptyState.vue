<script setup lang="ts">
defineProps<{ title: string; message?: string }>()
</script>

<template>
  <div class="empty">
    <strong>{{ title }}</strong>
    <p v-if="message">{{ message }}</p>
    <slot />
  </div>
</template>

<style scoped>
.empty {
  padding: var(--space-6) var(--space-4);
  text-align: center;
  color: var(--color-text-muted);
}
strong {
  color: var(--color-text);
}
p {
  margin: var(--space-1) 0 0;
}
</style>
