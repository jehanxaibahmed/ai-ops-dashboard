<script setup lang="ts">
import type { Job } from '@/shared/api/types'
import ProgressBar from '@/shared/components/ProgressBar.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatCurrency, formatDuration, formatRelativeTime, shortId } from '@/shared/utils/format'

defineProps<{ jobs: Job[]; selectedId?: string | null; now: number }>()
const emit = defineEmits<{ select: [id: string] }>()
const catalog = useCatalogStore()
</script>

<template>
  <div class="table-wrap">
    <table>
      <thead>
        <tr>
          <th>Job</th>
          <th>Pipeline</th>
          <th>Model</th>
          <th>Status</th>
          <th class="progress-col">Progress</th>
          <th class="num">Duration</th>
          <th class="num">Cost</th>
          <th class="num">Created</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="job in jobs"
          :key="job.id"
          :class="{ selected: job.id === selectedId }"
          tabindex="0"
          @click="emit('select', job.id)"
          @keydown.enter="emit('select', job.id)"
        >
          <td>
            <div class="doc">{{ job.documentName }}</div>
            <div class="muted mono">
              {{ shortId(job.id) }}<span v-if="job.attempt > 1"> · attempt {{ job.attempt }}</span>
            </div>
          </td>
          <td>{{ catalog.pipelineName(job.pipelineId) }}</td>
          <td class="muted">{{ catalog.modelName(job.model) }}</td>
          <td><StatusBadge :status="job.status" /></td>
          <td class="progress-col">
            <div class="progress-cell">
              <ProgressBar :value="job.progress" :status="job.status" />
              <span class="tabular muted">{{ job.progress }}%</span>
            </div>
          </td>
          <td class="num tabular">{{ formatDuration(job.durationSeconds) }}</td>
          <td class="num tabular">{{ formatCurrency(job.costUsd) }}</td>
          <td class="num muted" :title="job.createdAt">{{ formatRelativeTime(job.createdAt, now) }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.table-wrap {
  overflow-x: auto;
}
table {
  width: 100%;
  border-collapse: collapse;
  white-space: nowrap;
}
th {
  text-align: left;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
  padding: var(--space-2) var(--space-3);
  border-bottom: 1px solid var(--color-border);
}
td {
  padding: var(--space-2) var(--space-3);
  border-bottom: 1px solid var(--color-border);
  vertical-align: middle;
}
tbody tr {
  cursor: pointer;
}
tbody tr:hover {
  background: var(--color-surface-hover);
}
tbody tr.selected {
  background: var(--color-accent-soft);
}
tbody tr:focus-visible {
  outline: 2px solid var(--color-accent);
  outline-offset: -2px;
}
.num {
  text-align: right;
}
.doc {
  font-weight: 550;
}
.muted {
  color: var(--color-text-muted);
  font-size: 12px;
}
.mono {
  font-family: var(--font-mono);
}
.progress-col {
  width: 160px;
}
.progress-cell {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}
.progress-cell > :first-child {
  flex: 1;
}
</style>
