<script setup lang="ts">
import { computed } from 'vue'
import type { Job } from '@/shared/api/types'
import AppButton from '@/shared/components/AppButton.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatRelativeTime } from '@/shared/utils/format'

const props = defineProps<{ jobs: Job[]; selection: Set<string>; pending: Set<string>; now: number }>()
const emit = defineEmits<{ toggle: [id: string]; toggleAll: [ids: string[]]; retry: [id: string]; open: [id: string] }>()
const catalog = useCatalogStore()

const retryableIds = computed(() => props.jobs.filter((j) => j.canRetry && j.status === 'Failed').map((j) => j.id))
const allSelected = computed(() => retryableIds.value.length > 0 && retryableIds.value.every((id) => props.selection.has(id)))
</script>

<template>
  <div class="table-wrap">
    <table>
      <thead>
        <tr>
          <th class="check">
            <input
              type="checkbox"
              aria-label="Select all retryable"
              :checked="allSelected"
              :disabled="retryableIds.length === 0"
              @change="emit('toggleAll', retryableIds)"
            />
          </th>
          <th>Job</th>
          <th>Pipeline</th>
          <th>Error</th>
          <th class="num">Attempt</th>
          <th class="num">Failed</th>
          <th />
        </tr>
      </thead>
      <tbody>
        <tr v-for="job in jobs" :key="job.id">
          <td class="check">
            <input
              type="checkbox"
              :aria-label="`Select ${job.documentName}`"
              :checked="selection.has(job.id)"
              :disabled="!job.canRetry || job.status !== 'Failed'"
              @change="emit('toggle', job.id)"
            />
          </td>
          <td>
            <button class="link" @click="emit('open', job.id)">{{ job.documentName }}</button>
          </td>
          <td>{{ catalog.pipelineName(job.pipelineId) }}</td>
          <td>
            <template v-if="job.status === 'Failed' && job.failure">
              <span class="code">{{ job.failure.code }}</span>
              <span v-if="!job.failure.isTransient" class="perm" title="Permanent error">permanent</span>
            </template>
            <StatusBadge v-else :status="job.status" />
          </td>
          <td class="num tabular">{{ job.attempt }}</td>
          <td class="num muted">{{ job.completedAt ? formatRelativeTime(job.completedAt, now) : '—' }}</td>
          <td class="num">
            <AppButton
              v-if="job.status === 'Failed'"
              size="sm"
              :disabled="!job.canRetry"
              :loading="pending.has(job.id)"
              :title="job.canRetry ? 'Retry this job' : 'No attempts left'"
              @click="emit('retry', job.id)"
            >
              Retry
            </AppButton>
          </td>
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
}
.check {
  width: 32px;
}
.num {
  text-align: right;
}
.muted {
  color: var(--color-text-muted);
  font-size: 12px;
}
.link {
  border: none;
  background: none;
  padding: 0;
  color: var(--color-text);
  font-weight: 550;
  cursor: pointer;
}
.link:hover {
  color: var(--color-accent);
  text-decoration: underline;
}
.code {
  font-family: var(--font-mono);
  color: var(--color-danger);
}
.perm {
  margin-left: 6px;
  font-size: 11px;
  color: var(--color-text-muted);
}
</style>
