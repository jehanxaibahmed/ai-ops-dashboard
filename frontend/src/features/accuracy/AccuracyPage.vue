<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import type { AccuracyGroupBy } from '@/shared/api/types'
import AppButton from '@/shared/components/AppButton.vue'
import Card from '@/shared/components/Card.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import ChartCard from '@/shared/components/charts/ChartCard.vue'
import { seriesColor, useChartTheme } from '@/shared/components/charts/useChartTheme'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatDay } from '@/shared/utils/dates'
import { formatNumber, formatPercent, formatPointsChange } from '@/shared/utils/format'
import AccuracyRowsTable from './components/AccuracyRowsTable.vue'
import AccuracyTrendChart from './components/AccuracyTrendChart.vue'
import WorstFieldsList from './components/WorstFieldsList.vue'
import { useAccuracyStore } from './useAccuracyStore'

const store = useAccuracyStore()
const { report, loading, error, groupBy } = storeToRefs(store)
const catalog = useCatalogStore()
const theme = useChartTheme()

const GROUP_OPTIONS: AccuracyGroupBy[] = ['Pipeline', 'Model']

/** Fixed colour slot from catalog order, so a pipeline or model keeps its colour across views. */
const colorIndex = (key: string) => {
  const list = groupBy.value === 'Model' ? catalog.models.map((m) => m.id) : catalog.pipelines.map((p) => p.id)
  return Math.max(0, list.indexOf(key))
}
const label = (key: string) => (groupBy.value === 'Model' ? catalog.modelName(key) : catalog.pipelineName(key))
const color = (key: string) => seriesColor(theme.value, colorIndex(key))

const biggestDrop = computed(() => {
  const rows = report.value?.rows.filter((r) => r.recentChange !== null) ?? []
  return rows.reduce<(typeof rows)[number] | null>((worst, r) => (!worst || r.recentChange! < worst.recentChange! ? r : worst), null)
})

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
  <PageHeader title="Accuracy" subtitle="Field-level extraction accuracy from evaluation results">
    <template #actions>
      <div class="group" role="group" aria-label="Group by">
        <AppButton
          v-for="g in GROUP_OPTIONS"
          :key="g"
          size="sm"
          :variant="groupBy === g ? 'primary' : 'secondary'"
          :aria-pressed="groupBy === g"
          @click="store.setGroupBy(g)"
        >
          By {{ g.toLowerCase() }}
        </AppButton>
      </div>
    </template>
  </PageHeader>

  <p v-if="error" class="error" role="alert">
    {{ error }} <AppButton size="sm" @click="store.load()">Retry</AppButton>
  </p>

  <template v-if="report">
    <div class="stats">
      <StatCard label="Field accuracy" tone="success" :value="formatPercent(report.overall.accuracy)" :hint="`${formatNumber(report.overall.fieldsChecked)} fields checked`" />
      <StatCard label="Perfect extractions" :value="formatPercent(report.overall.perfectRate)" hint="Documents with every field right" />
      <StatCard label="Evaluations" :value="formatNumber(report.overall.evaluations)" />
      <StatCard
        label="Biggest recent drop"
        :tone="(biggestDrop?.recentChange ?? 0) <= -0.02 ? 'danger' : 'default'"
        :value="biggestDrop ? formatPointsChange(biggestDrop.recentChange) : '—'"
        :hint="biggestDrop ? `${label(biggestDrop.key)} · last 3 days` : undefined"
      />
    </div>

    <ChartCard :title="`Daily accuracy by ${groupBy.toLowerCase()}`" subtitle="UTC days. Hover for values and sample sizes." :loading="loading" class="block">
      <AccuracyTrendChart :daily="report.daily" :keys="report.keys" :color-index="colorIndex" :label="label" />
      <template #table>
        <table>
          <thead>
            <tr>
              <th>Day</th>
              <th v-for="k in report.keys" :key="k" class="num">{{ label(k) }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="d in report.daily" :key="d.date">
              <td>{{ formatDay(d.date) }}</td>
              <td v-for="k in report.keys" :key="k" class="num">
                {{ formatPercent(d.accuracy[k]) }} <span class="muted">({{ d.evaluations[k] ?? 0 }})</span>
              </td>
            </tr>
          </tbody>
        </table>
      </template>
    </ChartCard>

    <div class="grid">
      <Card :title="`By ${groupBy.toLowerCase()}`" subtitle="Flags drops of 2 points or more">
        <div class="scroll">
          <AccuracyRowsTable :rows="report.rows" :key-label="groupBy" :label="label" :color="color" />
        </div>
      </Card>
      <Card title="Most-missed fields" subtitle="Error rate per field, for the current filters">
        <WorstFieldsList :fields="report.worstFields" />
      </Card>
    </div>
  </template>
  <EmptyState v-else-if="!loading && !error" title="No evaluations yet" />
</template>

<style scoped>
.group {
  display: flex;
  gap: var(--space-1);
}
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
.scroll {
  overflow-x: auto;
}
.muted {
  color: var(--color-text-muted);
  font-size: 11px;
}
</style>
