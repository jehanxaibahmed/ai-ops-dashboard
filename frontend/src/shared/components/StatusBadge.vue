<script setup lang="ts">
import type { JobStatus } from '@/shared/api/types'

defineProps<{ status: JobStatus }>()
</script>

<template>
  <span class="badge" :class="status.toLowerCase()">
    <span class="dot" aria-hidden="true" />
    {{ status }}
  </span>
</template>

<style scoped>
.badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}
.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}
.queued {
  color: var(--color-neutral);
  background: var(--color-neutral-soft);
}
.running {
  color: var(--color-info);
  background: var(--color-info-soft);
}
.running .dot {
  animation: pulse 1.2s ease-in-out infinite;
}
.succeeded {
  color: var(--color-success);
  background: var(--color-success-soft);
}
.failed {
  color: var(--color-danger);
  background: var(--color-danger-soft);
}
@keyframes pulse {
  50% {
    opacity: 0.3;
  }
}
@media (prefers-reduced-motion: reduce) {
  .running .dot {
    animation: none;
  }
}
</style>
