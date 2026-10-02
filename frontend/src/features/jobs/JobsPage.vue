<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useRoute, useRouter } from 'vue-router'
import { JOB_STATUSES, type JobStatus } from '@/shared/api/types'
import AppButton from '@/shared/components/AppButton.vue'
import Card from '@/shared/components/Card.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import { useNow } from '@/shared/composables/useNow'
import { useRetry } from '@/shared/composables/useRetry'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import JobDetailPanel from '@/shared/components/jobs/JobDetailPanel.vue'
import JobsTable from './components/JobsTable.vue'
import StatusTabs from './components/StatusTabs.vue'
import { useJobsStore } from './useJobsStore'

const store = useJobsStore()
const { items, total, page, pageSize, loading, error, paused, missedWhilePaused, selected, selectedId } = storeToRefs(store)
const catalog = useCatalogStore()
const now = useNow()
const { pending, retryOne } = useRetry((jobs) => jobs.forEach(store.applyUpdate))

const route = useRoute()
const router = useRouter()

// The status tab lives in the URL next to the global filters, so a filtered view can be shared.
const status = computed<JobStatus | null>({
  get: () => {
    const value = route.query.status
    return typeof value === 'string' && (JOB_STATUSES as readonly string[]).includes(value) ? (value as JobStatus) : null
  },
  set: (value) => {
    const { status: _status, ...rest } = route.query
    void router.replace({ query: value ? { ...rest, status: value } : rest })
  },
})

const filters = useGlobalFilters()
const applyFilters = () => store.setQuery({ ...filters.query.value, status: status.value ? [status.value] : undefined })
watch([filters.key, status], applyFilters)

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  void applyFilters()
  disconnect = store.connect()
})
onBeforeUnmount(() => disconnect?.())
</script>

<template>
  <PageHeader title="Jobs" subtitle="Every job across all pipelines, updated live">
    <template #actions>
      <AppButton :variant="paused ? 'primary' : 'secondary'" @click="store.setPaused(!paused)">
        {{ paused ? `Resume live${missedWhilePaused ? ` (${missedWhilePaused} new)` : ''}` : 'Pause live' }}
      </AppButton>
    </template>
  </PageHeader>

  <Card>
    <template #actions>
      <StatusTabs v-model="status" />
    </template>

    <p v-if="error" class="error" role="alert">
      {{ error }} <AppButton size="sm" @click="store.load()">Retry</AppButton>
    </p>
    <EmptyState v-else-if="!loading && items.length === 0" title="No jobs" message="Nothing matches this filter yet." />
    <JobsTable v-else :jobs="items" :selected-id="selectedId" :now="now" :class="{ loading }" @select="store.select" />

    <PaginationBar :page="page" :page-size="pageSize" :total="total" @update:page="store.setPage" />
  </Card>

  <JobDetailPanel v-if="selected" :job="selected" @close="store.select(null)">
    <template v-if="selected.status === 'Failed'" #actions>
      <AppButton variant="primary" :disabled="!selected.canRetry" :loading="pending.has(selected.id)" @click="retryOne(selected.id)">
        Retry job
      </AppButton>
    </template>
  </JobDetailPanel>
</template>

<style scoped>
.error {
  color: var(--color-danger);
}
.loading {
  opacity: 0.6;
}
</style>
