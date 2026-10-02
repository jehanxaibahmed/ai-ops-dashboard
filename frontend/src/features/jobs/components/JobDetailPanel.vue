<script setup lang="ts">
import type { Job } from '@/shared/api/types'
import ProgressBar from '@/shared/components/ProgressBar.vue'
import SidePanel from '@/shared/components/SidePanel.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatCurrency, formatDateTime, formatDuration, formatNumber } from '@/shared/utils/format'

defineProps<{ job: Job }>()
const emit = defineEmits<{ close: [] }>()
const catalog = useCatalogStore()
</script>

<template>
  <SidePanel :title="job.documentName" @close="emit('close')">
    <div class="status-row">
      <StatusBadge :status="job.status" />
      <span class="muted">Attempt {{ job.attempt }}</span>
    </div>
    <ProgressBar :value="job.progress" :status="job.status" />

    <div v-if="job.failure" class="failure" role="alert">
      <div class="failure-code">{{ job.failure.code }}</div>
      <div>{{ job.failure.message }}</div>
      <div class="muted">{{ job.failure.isTransient ? 'Transient: likely to succeed on retry' : 'Permanent: needs a fix before retrying' }}</div>
    </div>

    <dl>
      <dt>Job ID</dt>
      <dd class="mono">{{ job.id }}</dd>
      <dt>Pipeline</dt>
      <dd>{{ catalog.pipelineName(job.pipelineId) }}</dd>
      <dt>Model</dt>
      <dd>{{ catalog.modelName(job.model) }}</dd>
      <dt>Created</dt>
      <dd>{{ formatDateTime(job.createdAt) }}</dd>
      <dt>Started</dt>
      <dd>{{ job.startedAt ? formatDateTime(job.startedAt) : '—' }}</dd>
      <dt>Completed</dt>
      <dd>{{ job.completedAt ? formatDateTime(job.completedAt) : '—' }}</dd>
      <dt>Duration</dt>
      <dd>{{ formatDuration(job.durationSeconds) }}</dd>
      <dt>Input tokens</dt>
      <dd class="tabular">{{ formatNumber(job.inputTokens) }}</dd>
      <dt>Output tokens</dt>
      <dd class="tabular">{{ formatNumber(job.outputTokens) }}</dd>
      <dt>Cost</dt>
      <dd class="tabular">{{ formatCurrency(job.costUsd) }}</dd>
    </dl>

    <template v-if="$slots.actions" #footer>
      <slot name="actions" />
    </template>
  </SidePanel>
</template>

<style scoped>
.status-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-2);
}
.failure {
  margin-top: var(--space-4);
  padding: var(--space-3);
  border-radius: var(--radius-sm);
  background: var(--color-danger-soft);
  display: grid;
  gap: 2px;
}
.failure-code {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--color-danger);
}
dl {
  display: grid;
  grid-template-columns: max-content 1fr;
  gap: var(--space-2) var(--space-4);
  margin: var(--space-5) 0 0;
}
dt {
  color: var(--color-text-muted);
}
dd {
  margin: 0;
  overflow-wrap: anywhere;
}
.muted {
  color: var(--color-text-muted);
  font-size: 12px;
}
.mono {
  font-family: var(--font-mono);
  font-size: 12px;
}
</style>
