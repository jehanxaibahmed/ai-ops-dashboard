<script setup lang="ts">
import { JOB_STATUSES, type JobStatus } from '@/shared/api/types'

const model = defineModel<JobStatus | null>({ required: true })
const options: (JobStatus | null)[] = [null, ...JOB_STATUSES]
</script>

<template>
  <div class="tabs" role="tablist" aria-label="Filter by status">
    <button
      v-for="option in options"
      :key="option ?? 'all'"
      role="tab"
      :aria-selected="model === option"
      :class="{ active: model === option }"
      @click="model = option"
    >
      {{ option ?? 'All' }}
    </button>
  </div>
</template>

<style scoped>
.tabs {
  display: inline-flex;
  flex-wrap: wrap;
  gap: 2px;
  padding: 3px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
}
button {
  border: none;
  border-radius: 4px;
  padding: 4px 12px;
  background: none;
  color: var(--color-text-muted);
  cursor: pointer;
  font-weight: 550;
}
button:hover {
  color: var(--color-text);
}
button.active {
  background: var(--color-accent-soft);
  color: var(--color-accent);
}
</style>
