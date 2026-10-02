<script setup lang="ts">
import type { CostBreakdownRow } from '@/shared/api/types'
import { formatCompact, formatCurrency, formatNumber, formatPercent } from '@/shared/utils/format'

defineProps<{ rows: CostBreakdownRow[]; keyLabel: string; label: (key: string) => string }>()
</script>

<template>
  <table>
    <thead>
      <tr>
        <th>{{ keyLabel }}</th>
        <th class="num">Jobs</th>
        <th class="num">Tokens</th>
        <th class="num">Cost</th>
        <th class="num">Per job</th>
        <th class="num">Share</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="row in rows" :key="row.key">
        <td>{{ label(row.key) }}</td>
        <td class="num">{{ formatNumber(row.jobs) }}</td>
        <td class="num" :title="`${formatNumber(row.inputTokens)} in · ${formatNumber(row.outputTokens)} out`">
          {{ formatCompact(row.inputTokens + row.outputTokens) }}
        </td>
        <td class="num strong">{{ formatCurrency(row.costUsd) }}</td>
        <td class="num">{{ formatCurrency(row.averageCostPerJobUsd ?? 0) }}</td>
        <td class="num">{{ formatPercent(row.share) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
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
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
td {
  padding: var(--space-2);
  border-bottom: 1px solid var(--color-border);
}
.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
.strong {
  font-weight: 600;
}
</style>
