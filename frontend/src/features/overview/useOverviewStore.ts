import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import { getJobSummary, listJobs } from '@/shared/api/jobs'
import type { Job, JobQuery, JobSummary } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { matchesQuery } from '@/shared/utils/jobQuery'
import { throttle } from '@/shared/utils/throttle'

export const useOverviewStore = defineStore('overview', () => {
  const summary = ref<JobSummary | null>(null)
  const active = shallowRef<Job[]>([])
  const error = ref<string | null>(null)
  const query = ref<JobQuery>({})

  /** Active jobs ignore the date range: anything running now is relevant. */
  const activeQuery = (): JobQuery => ({ pipeline: query.value.pipeline, model: query.value.model, status: ['Running', 'Queued'] })

  async function loadSummary() {
    try {
      summary.value = await getJobSummary(query.value)
      error.value = null
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load summary'
    }
  }

  async function loadActive() {
    const result = await listJobs(activeQuery(), 1, 50)
    active.value = result.items
  }

  const running = computed(() => active.value.filter((j) => j.status === 'Running'))
  const queued = computed(() => active.value.filter((j) => j.status === 'Queued'))

  // Summary is an aggregate, so refetch it at most every few seconds instead of on every event.
  const refreshSummary = throttle(() => void loadSummary(), 3000)

  function applyUpdate(job: Job) {
    const rest = active.value.filter((j) => j.id !== job.id)
    active.value = matchesQuery(job, activeQuery()) ? [job, ...rest] : rest
    refreshSummary()
  }

  function setQuery(next: JobQuery) {
    query.value = next
    void loadSummary()
    void loadActive()
  }

  function connect() {
    const off = onJobUpdated(applyUpdate, () => {
      void loadSummary()
      void loadActive()
    })
    return () => {
      off()
      refreshSummary.cancel()
    }
  }

  return { summary, running, queued, error, setQuery, connect }
})
