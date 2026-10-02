import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import { getJob, listJobs } from '@/shared/api/jobs'
import type { Job, JobQuery } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { matchesQuery } from '@/shared/utils/jobQuery'
import { mergeJobUpdate } from './liveMerge'

export const useJobsStore = defineStore('jobs', () => {
  const query = ref<JobQuery>({})
  const page = ref(1)
  const pageSize = ref(25)

  const items = shallowRef<Job[]>([])
  const total = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  /** When paused, live updates are counted but not applied, so the table holds still. */
  const paused = ref(false)
  const missedWhilePaused = ref(0)

  const selectedId = ref<string | null>(null)
  const selectedFallback = shallowRef<Job | null>(null)
  const selected = computed(
    () => items.value.find((j) => j.id === selectedId.value) ?? selectedFallback.value,
  )

  let requestSeq = 0

  async function load() {
    const seq = ++requestSeq
    loading.value = true
    error.value = null
    try {
      const result = await listJobs(query.value, page.value, pageSize.value)
      if (seq !== requestSeq) return
      items.value = result.items
      total.value = result.total
    } catch (e) {
      if (seq === requestSeq) error.value = e instanceof Error ? e.message : 'Failed to load jobs'
    } finally {
      if (seq === requestSeq) loading.value = false
    }
  }

  function setQuery(next: JobQuery) {
    query.value = next
    page.value = 1
    return load()
  }

  function setPage(next: number) {
    page.value = next
    return load()
  }

  function applyUpdate(job: Job) {
    if (selectedFallback.value?.id === job.id) selectedFallback.value = job
    if (paused.value) {
      if (matchesQuery(job, query.value)) missedWhilePaused.value++
      return
    }
    const result = mergeJobUpdate(items.value, job, {
      matches: matchesQuery(job, query.value),
      isFirstPage: page.value === 1,
      pageSize: pageSize.value,
    })
    items.value = result.items
    if (result.added) total.value++
  }

  function setPaused(value: boolean) {
    paused.value = value
    if (!value && missedWhilePaused.value > 0) {
      missedWhilePaused.value = 0
      void load()
    }
  }

  async function select(id: string | null) {
    selectedId.value = id
    selectedFallback.value = null
    if (id && !items.value.some((j) => j.id === id)) {
      selectedFallback.value = await getJob(id)
    }
  }

  /** Starts receiving live updates. Call the returned function to stop. */
  function connect() {
    return onJobUpdated(applyUpdate, () => void load())
  }

  return {
    query,
    page,
    pageSize,
    items,
    total,
    loading,
    error,
    paused,
    missedWhilePaused,
    selected,
    selectedId,
    load,
    setQuery,
    setPage,
    setPaused,
    select,
    connect,
    applyUpdate,
  }
})
