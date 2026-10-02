<script setup lang="ts">
import type { FailurePipelineStat } from '@/shared/api/types'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatNumber, formatPercent } from '@/shared/utils/format'

defineProps<{ pipelines: FailurePipelineStat[] }>()
const catalog = useCatalogStore()
</script>

<template>
  <table>
    <thead>
      <tr>
        <th>Pipeline</th>
        <th class="num">Failed</th>
        <th class="num">Finished</th>
        <th class="num">Failure rate</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="p in pipelines" :key="p.pipelineId">
        <td>{{ catalog.pipelineName(p.pipelineId) }}</td>
        <td class="num tabular">{{ formatNumber(p.failed) }}</td>
        <td class="num tabular muted">{{ formatNumber(p.finished) }}</td>
        <td class="num tabular rate">{{ formatPercent(p.failureRate) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
table {
  width: 100%;
  border-collapse: collapse;
}
th {
  text-align: left;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-muted);
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
td {
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
.num {
  text-align: right;
}
.muted {
  color: var(--color-text-muted);
}
.rate {
  font-weight: 600;
  color: var(--color-danger);
}
</style>
