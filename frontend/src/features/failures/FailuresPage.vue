<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { getJob } from '@/shared/api/jobs'
import type { Job } from '@/shared/api/types'
import AppButton from '@/shared/components/AppButton.vue'
import Card from '@/shared/components/Card.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import StatCard from '@/shared/components/StatCard.vue'
import JobDetailPanel from '@/shared/components/jobs/JobDetailPanel.vue'
import { useNow } from '@/shared/composables/useNow'
import { useRetry } from '@/shared/composables/useRetry'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatNumber } from '@/shared/utils/format'
import FailedJobsTable from './components/FailedJobsTable.vue'
import FailureCodeList from './components/FailureCodeList.vue'
import PipelineFailureRates from './components/PipelineFailureRates.vue'
import { useFailuresStore } from './useFailuresStore'

const store = useFailuresStore()
const { visible, total, page, pageSize, breakdown, loading, error, selection, selectedRetryable, transientRetryable, codeFilter } =
  storeToRefs(store)
const catalog = useCatalogStore()
const now = useNow()

const { pending, bulkPending, retryOne, retryMany } = useRetry((jobs) => {
  jobs.forEach(store.applyUpdate)
  store.clearSelection()
})

const openId = ref<string | null>(null)
const openFallback = ref<Job | null>(null)
const openJob = computed(() => store.items.find((j) => j.id === openId.value) ?? openFallback.value)
async function open(id: string) {
  openId.value = id
  openFallback.value = store.items.some((j) => j.id === id) ? null : await getJob(id)
}

const filters = useGlobalFilters()
watch(filters.key, () => store.setBaseQuery(filters.query.value))

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  void store.setBaseQuery(filters.query.value)
  disconnect = store.connect()
})
onBeforeUnmount(() => disconnect?.())
</script>

<template>
  <PageHeader title="Failures" subtitle="What is failing, why, and what to retry">
    <template #actions>
      <AppButton
        :disabled="transientRetryable.length === 0"
        :loading="bulkPending"
        @click="retryMany(transientRetryable.map((j) => j.id))"
      >
        Retry transient on page ({{ transientRetryable.length }})
      </AppButton>
      <AppButton
        variant="primary"
        :disabled="selectedRetryable.length === 0"
        :loading="bulkPending"
        @click="retryMany(selectedRetryable.map((j) => j.id))"
      >
        Retry selected ({{ selectedRetryable.length }})
      </AppButton>
    </template>
  </PageHeader>

  <p v-if="error" class="error" role="alert">
    {{ error }} <AppButton size="sm" @click="store.load()">Retry</AppButton>
  </p>

  <div class="stats">
    <StatCard label="Failed jobs" tone="danger" :value="breakdown ? formatNumber(breakdown.totalFailed) : '—'" />
    <StatCard label="Retryable" :value="breakdown ? formatNumber(breakdown.retryable) : '—'" hint="Attempts left" />
    <StatCard label="Distinct errors" :value="breakdown ? formatNumber(breakdown.byCode.length) : '—'" />
    <StatCard
      label="Top error"
      :value="breakdown?.byCode[0]?.code ?? '—'"
      :hint="breakdown?.byCode[0] ? `${breakdown.byCode[0].count} jobs` : undefined"
    />
  </div>

  <div class="grid">
    <Card title="By error code" subtitle="Click a code to filter the list">
      <FailureCodeList v-if="breakdown?.byCode.length" :codes="breakdown.byCode" :active="codeFilter" @select="store.setCode" />
      <EmptyState v-else title="No failures" />
    </Card>
    <Card title="By pipeline" subtitle="Share of finished jobs that failed">
      <PipelineFailureRates v-if="breakdown" :pipelines="breakdown.byPipeline" />
    </Card>
  </div>

  <Card title="Failed jobs" :subtitle="codeFilter ? `Only ${codeFilter}` : 'Newest first'" class="list">
    <template v-if="codeFilter" #actions>
      <AppButton size="sm" variant="ghost" @click="store.setCode(null)">Clear filter</AppButton>
    </template>
    <EmptyState v-if="!loading && visible.length === 0" title="Nothing to show" message="No failed jobs match." />
    <FailedJobsTable
      v-else
      :jobs="visible"
      :selection="selection"
      :pending="pending"
      :now="now"
      @toggle="store.toggle"
      @toggle-all="store.toggleAll"
      @retry="retryOne"
      @open="open"
    />
    <PaginationBar :page="page" :page-size="pageSize" :total="total" @update:page="store.setPage" />
  </Card>

  <JobDetailPanel v-if="openJob" :job="openJob" @close="openId = null">
    <template v-if="openJob.status === 'Failed'" #actions>
      <AppButton variant="primary" :disabled="!openJob.canRetry" :loading="pending.has(openJob.id)" @click="retryOne(openJob.id)">
        Retry job
      </AppButton>
    </template>
  </JobDetailPanel>
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
  margin-bottom: var(--space-4);
}
</style>
