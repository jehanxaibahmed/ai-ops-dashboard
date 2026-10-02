<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import type { JobStatus } from '@/shared/api/types'
import AppButton from '@/shared/components/AppButton.vue'
import Card from '@/shared/components/Card.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import { useNow } from '@/shared/composables/useNow'
import { useCatalogStore } from '@/shared/stores/catalog'
import JobDetailPanel from './components/JobDetailPanel.vue'
import JobsTable from './components/JobsTable.vue'
import StatusTabs from './components/StatusTabs.vue'
import { useJobsStore } from './useJobsStore'

const store = useJobsStore()
const { items, total, page, pageSize, loading, error, paused, missedWhilePaused, selected, selectedId } = storeToRefs(store)
const catalog = useCatalogStore()
const now = useNow()

const status = computed<JobStatus | null>({
  get: () => store.query.status?.[0] ?? null,
  set: (value) => void store.setQuery({ ...store.query, status: value ? [value] : undefined }),
})

let disconnect: (() => void) | undefined
onMounted(() => {
  void catalog.ensureLoaded()
  void store.load()
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

  <JobDetailPanel v-if="selected" :job="selected" @close="store.select(null)" />
</template>

<style scoped>
.error {
  color: var(--color-danger);
}
.loading {
  opacity: 0.6;
}
</style>
