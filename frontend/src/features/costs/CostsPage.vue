<script setup lang="ts">
import { onBeforeUnmount, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import AppButton from '@/shared/components/AppButton.vue'
import Card from '@/shared/components/Card.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import ChartCard from '@/shared/components/charts/ChartCard.vue'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatDay } from '@/shared/utils/dates'
import { formatCompact, formatCurrency, formatPercent } from '@/shared/utils/format'
import BreakdownTable from './components/BreakdownTable.vue'
import CostByModelChart from './components/CostByModelChart.vue'
import DailyCostChart from './components/DailyCostChart.vue'
import { useCostsStore } from './useCostsStore'

const store = useCostsStore()
const { report, loading, error } = storeToRefs(store)
const catalog = useCatalogStore()

/** Fixed colour slot per model, taken from catalog order. */
const colorIndex = (model: string) => Math.max(0, catalog.models.findIndex((m) => m.id === model))

const filters = useGlobalFilters()
watch(filters.key, () => store.setQuery(filters.query.value))

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  void store.setQuery(filters.query.value)
  disconnect = store.connect()
})
onBeforeUnmount(() => disconnect?.())
</script>

<template>
  <PageHeader title="Costs" subtitle="Model spend by day, model and pipeline" />

  <p v-if="error" class="error" role="alert">
    {{ error }} <AppButton size="sm" @click="store.load()">Retry</AppButton>
  </p>

  <template v-if="report">
    <div class="stats">
      <StatCard label="Total spend" :value="formatCurrency(report.totals.totalCostUsd)" :hint="`${report.totals.jobs} jobs`" />
      <StatCard label="Average per job" :value="formatCurrency(report.totals.averageCostPerJobUsd ?? 0)" />
      <StatCard
        label="Tokens"
        :value="formatCompact(report.totals.inputTokens + report.totals.outputTokens)"
        :hint="`${formatCompact(report.totals.inputTokens)} in · ${formatCompact(report.totals.outputTokens)} out`"
      />
      <StatCard
        label="Spent on failed jobs"
        tone="danger"
        :value="formatCurrency(report.totals.failedCostUsd)"
        :hint="report.totals.totalCostUsd ? `${formatPercent(report.totals.failedCostUsd / report.totals.totalCostUsd)} of spend` : undefined"
      />
    </div>

    <ChartCard title="Daily cost by model" subtitle="UTC days, stacked by model" :loading="loading" class="block">
      <DailyCostChart :daily="report.daily" :models="report.models" :color-index="colorIndex" :label="catalog.modelName" />
      <template #table>
        <table>
          <thead>
            <tr>
              <th>Day</th>
              <th v-for="m in report.models" :key="m" class="num">{{ catalog.modelName(m) }}</th>
              <th class="num">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="d in report.daily" :key="d.date">
              <td>{{ formatDay(d.date) }}</td>
              <td v-for="m in report.models" :key="m" class="num">{{ formatCurrency(d.byModel[m] ?? 0) }}</td>
              <td class="num"><strong>{{ formatCurrency(d.totalCostUsd) }}</strong></td>
            </tr>
          </tbody>
        </table>
      </template>
    </ChartCard>

    <div class="grid">
      <ChartCard title="Cost by model" :loading="loading">
        <CostByModelChart :rows="report.byModel" :color-index="colorIndex" :label="catalog.modelName" />
        <template #table>
          <BreakdownTable :rows="report.byModel" key-label="Model" :label="catalog.modelName" />
        </template>
      </ChartCard>
      <Card title="Cost by pipeline" subtitle="Sorted by spend">
        <BreakdownTable :rows="report.byPipeline" key-label="Pipeline" :label="catalog.pipelineName" />
      </Card>
    </div>
  </template>
  <EmptyState v-else-if="!loading && !error" title="No cost data" />
</template>

<style scoped>
.error {
  color: var(--color-danger);
}
.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}
.block {
  margin-bottom: var(--space-4);
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 420px), 1fr));
  gap: var(--space-4);
}
.grid > * {
  overflow-x: auto;
}
</style>
