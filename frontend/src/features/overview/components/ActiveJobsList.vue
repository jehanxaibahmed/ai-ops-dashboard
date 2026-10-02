<script setup lang="ts">
import type { Job } from '@/shared/api/types'
import EmptyState from '@/shared/components/EmptyState.vue'
import ProgressBar from '@/shared/components/ProgressBar.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatCurrency } from '@/shared/utils/format'

defineProps<{ jobs: Job[] }>()
const catalog = useCatalogStore()
</script>

<template>
  <EmptyState v-if="jobs.length === 0" title="Idle" message="No jobs are running right now." />
  <TransitionGroup v-else tag="ul" name="row" class="list">
    <li v-for="job in jobs" :key="job.id">
      <div class="top">
        <span class="doc">{{ job.documentName }}</span>
        <span class="tabular muted">{{ job.progress }}%</span>
      </div>
      <ProgressBar :value="job.progress" :status="job.status" />
      <div class="meta muted">
        {{ catalog.pipelineName(job.pipelineId) }} · {{ catalog.modelName(job.model) }}
        <span class="tabular">· {{ formatCurrency(job.costUsd) }}</span>
      </div>
    </li>
  </TransitionGroup>
</template>

<style scoped>
.list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: var(--space-3);
}
.top {
  display: flex;
  justify-content: space-between;
  gap: var(--space-2);
  margin-bottom: 4px;
}
.doc {
  font-weight: 550;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.meta {
  margin-top: 4px;
}
.muted {
  color: var(--color-text-muted);
  font-size: 12px;
}
.row-enter-from,
.row-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
.row-enter-active,
.row-leave-active {
  transition: all 0.25s ease;
}
</style>
