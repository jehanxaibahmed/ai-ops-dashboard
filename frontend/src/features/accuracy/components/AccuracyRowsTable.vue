<script setup lang="ts">
import type { AccuracyRow } from '@/shared/api/types'
import { formatNumber, formatPercent, formatPointsChange } from '@/shared/utils/format'

defineProps<{ rows: AccuracyRow[]; keyLabel: string; label: (key: string) => string; color: (key: string) => string }>()

/** A drop of 2 points or more is worth flagging. */
const DROP_THRESHOLD = -0.02
</script>

<template>
  <table>
    <thead>
      <tr>
        <th>{{ keyLabel }}</th>
        <th class="num">Accuracy</th>
        <th class="num">Evaluations</th>
        <th class="num">Last 3 days vs before</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="row in rows" :key="row.key">
        <td>
          <span class="key" :style="{ background: color(row.key) }" aria-hidden="true" />
          {{ label(row.key) }}
        </td>
        <td class="num strong">{{ formatPercent(row.accuracy) }}</td>
        <td class="num">{{ formatNumber(row.evaluations) }}</td>
        <td class="num">
          <span :class="{ drop: (row.recentChange ?? 0) <= DROP_THRESHOLD }">
            <span v-if="(row.recentChange ?? 0) <= DROP_THRESHOLD" aria-hidden="true">▼ </span>
            {{ formatPointsChange(row.recentChange) }}
          </span>
        </td>
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
.key {
  display: inline-block;
  width: 14px;
  height: 2px;
  margin-right: 6px;
  vertical-align: middle;
  border-radius: 1px;
}
.drop {
  color: var(--color-danger);
  font-weight: 600;
}
</style>
