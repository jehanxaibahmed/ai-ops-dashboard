<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import Card from '@/shared/components/Card.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import { RANGE_LABELS } from '@/shared/filters/filters'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatCurrency, formatDuration, formatNumber, formatPercent } from '@/shared/utils/format'
import ActiveJobsList from './components/ActiveJobsList.vue'
import { useOverviewStore } from './useOverviewStore'

const store = useOverviewStore()
const { summary, running, queued, error } = storeToRefs(store)
const catalog = useCatalogStore()

const filters = useGlobalFilters()
const subtitle = computed(() => {
  const scope = filters.filters.value.pipelines.length || filters.filters.value.models.length ? 'selected pipelines and models' : 'all pipelines'
  return `${RANGE_LABELS[filters.filters.value.range]} across ${scope}`
})
watch(filters.key, () => store.setQuery(filters.query.value))

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  store.setQuery(filters.query.value)
  disconnect = store.connect()
})
onBeforeUnmount(() => disconnect?.())
</script>

<template>
  <PageHeader title="Overview" :subtitle="subtitle" />

  <p v-if="error" class="error" role="alert">{{ error }}</p>

  <div class="stats">
    <StatCard label="Jobs" :value="summary ? formatNumber(summary.total) : '—'" />
    <StatCard label="Running" tone="info" :value="summary ? formatNumber(summary.running) : '—'" :hint="summary ? `${summary.queued} queued` : undefined" />
    <StatCard label="Success rate" tone="success" :value="formatPercent(summary?.successRate)" />
    <StatCard label="Failed" tone="danger" :value="summary ? formatNumber(summary.failed) : '—'" />
    <StatCard label="Avg duration" :value="formatDuration(summary?.averageDurationSeconds)" />
    <StatCard label="Cost" :value="summary ? formatCurrency(summary.totalCostUsd) : '—'" />
  </div>

  <div class="grid">
    <Card title="Running now" :subtitle="`${running.length} running · ${queued.length} queued`">
      <ActiveJobsList :jobs="running" />
    </Card>
  </div>
</template>

<style scoped>
.error {
  color: var(--color-danger);
}
.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 380px), 1fr));
  gap: var(--space-4);
}
</style>
