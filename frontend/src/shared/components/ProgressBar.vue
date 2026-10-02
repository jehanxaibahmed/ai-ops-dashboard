<script setup lang="ts">
import { computed } from 'vue'
import type { JobStatus } from '@/shared/api/types'

const props = defineProps<{ value: number; status?: JobStatus }>()
const width = computed(() => `${Math.max(0, Math.min(100, props.value))}%`)
</script>

<template>
  <div
    class="progress"
    role="progressbar"
    :aria-valuenow="value"
    aria-valuemin="0"
    aria-valuemax="100"
    :class="status?.toLowerCase()"
  >
    <div class="fill" :style="{ width }" />
  </div>
</template>

<style scoped>
.progress {
  height: 6px;
  min-width: 60px;
  border-radius: 999px;
  background: var(--color-neutral-soft);
  overflow: hidden;
}
.fill {
  height: 100%;
  border-radius: inherit;
  background: var(--color-info);
  transition: width 0.4s ease;
}
.succeeded .fill {
  background: var(--color-success);
}
.failed .fill {
  background: var(--color-danger);
}
</style>
