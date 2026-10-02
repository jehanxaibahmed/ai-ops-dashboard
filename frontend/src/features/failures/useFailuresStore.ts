import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import { getFailureBreakdown, listJobs } from '@/shared/api/jobs'
import type { FailureBreakdown, Job, JobQuery } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { matchesQuery } from '@/shared/utils/jobQuery'
import { mergeJobUpdate } from '@/shared/utils/liveMerge'
import { throttle } from '@/shared/utils/throttle'

export const useFailuresStore = defineStore('failures', () => {
  /** Extra filters (pipeline, date) layered on top of status = Failed. */
  const baseQuery = ref<JobQuery>({})
  const codeFilter = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(25)

  const items = shallowRef<Job[]>([])
  const total = ref(0)
  const breakdown = ref<FailureBreakdown | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selection = ref(new Set<string>())

  const query = computed<JobQuery>(() => ({ ...baseQuery.value, status: ['Failed'] }))

  // The API filters by status, not error code, so the code filter is applied client-side
  // on the loaded page. Fine for a demo-sized dataset.
  const visible = computed(() =>
    codeFilter.value ? items.value.filter((j) => j.failure?.code === codeFilter.value) : items.value,
  )
  const selectedRetryable = computed(() => visible.value.filter((j) => selection.value.has(j.id) && j.canRetry))
  const transientRetryable = computed(() => visible.value.filter((j) => j.status === 'Failed' && j.canRetry && j.failure?.isTransient))

  let seq = 0
  async function load() {
    const current = ++seq
    loading.value = true
    error.value = null
    try {
      const [list, stats] = await Promise.all([
        listJobs(query.value, page.value, pageSize.value),
        getFailureBreakdown(baseQuery.value),
      ])
      if (current !== seq) return
      items.value = list.items
      total.value = list.total
      breakdown.value = stats
      selection.value = new Set([...selection.value].filter((id) => list.items.some((j) => j.id === id)))
    } catch (e) {
      if (current === seq) error.value = e instanceof Error ? e.message : 'Failed to load failures'
    } finally {
      if (current === seq) loading.value = false
    }
  }

  const refreshBreakdown = throttle(async () => {
    try {
      breakdown.value = await getFailureBreakdown(baseQuery.value)
    } catch {
      // keep the last good breakdown
    }
  }, 4000)

  function applyUpdate(job: Job) {
    const wasVisible = items.value.some((j) => j.id === job.id)
    const result = mergeJobUpdate(items.value, job, {
      matches: matchesQuery(job, query.value),
      isFirstPage: page.value === 1,
      pageSize: pageSize.value,
    })
    items.value = result.items
    if (result.added) total.value++
    if (job.status === 'Failed' || wasVisible) refreshBreakdown()
  }

  function toggle(id: string) {
    const next = new Set(selection.value)
    if (next.has(id)) next.delete(id)
    else next.add(id)
    selection.value = next
  }

  function toggleAll(ids: string[]) {
    const allSelected = ids.every((id) => selection.value.has(id))
    selection.value = allSelected ? new Set() : new Set(ids)
  }

  function clearSelection() {
    selection.value = new Set()
  }

  function setCode(code: string | null) {
    codeFilter.value = codeFilter.value === code ? null : code
    clearSelection()
  }

  function setPage(next: number) {
    page.value = next
    return load()
  }

  function setBaseQuery(next: JobQuery) {
    baseQuery.value = next
    page.value = 1
    return load()
  }

  function connect() {
    const off = onJobUpdated(applyUpdate, () => void load())
    return () => {
      off()
      refreshBreakdown.cancel()
    }
  }

  return {
    baseQuery,
    codeFilter,
    page,
    pageSize,
    items,
    visible,
    total,
    breakdown,
    loading,
    error,
    selection,
    selectedRetryable,
    transientRetryable,
    load,
    applyUpdate,
    toggle,
    toggleAll,
    clearSelection,
    setCode,
    setPage,
    setBaseQuery,
    connect,
  }
})
