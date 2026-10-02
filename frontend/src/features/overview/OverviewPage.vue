<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import Card from '@/shared/components/Card.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatCurrency, formatDuration, formatNumber, formatPercent } from '@/shared/utils/format'
import ActiveJobsList from './components/ActiveJobsList.vue'
import { useOverviewStore } from './useOverviewStore'

const store = useOverviewStore()
const { summary, running, queued, error } = storeToRefs(store)
const catalog = useCatalogStore()

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  disconnect = store.connect()
})
onBeforeUnmount(() => disconnect?.())
</script>

<template>
  <PageHeader title="Overview" subtitle="Last 24 hours across all pipelines" />

  <p v-if="error" class="error" role="alert">{{ error }}</p>

  <div class="stats">
    <StatCard label="Jobs (24h)" :value="summary ? formatNumber(summary.total) : '—'" />
    <StatCard label="Running" tone="info" :value="summary ? formatNumber(summary.running) : '—'" :hint="summary ? `${summary.queued} queued` : undefined" />
    <StatCard label="Success rate" tone="success" :value="formatPercent(summary?.successRate)" />
    <StatCard label="Failed" tone="danger" :value="summary ? formatNumber(summary.failed) : '—'" />
    <StatCard label="Avg duration" :value="formatDuration(summary?.averageDurationSeconds)" />
    <StatCard label="Cost (24h)" :value="summary ? formatCurrency(summary.totalCostUsd) : '—'" />
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
