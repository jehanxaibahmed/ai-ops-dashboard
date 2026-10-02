import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import { getJobSummary, listJobs } from '@/shared/api/jobs'
import type { Job, JobSummary } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { throttle } from '@/shared/utils/throttle'

const DAY_MS = 24 * 60 * 60 * 1000

export const useOverviewStore = defineStore('overview', () => {
  const summary = ref<JobSummary | null>(null)
  const active = shallowRef<Job[]>([])
  const error = ref<string | null>(null)

  async function loadSummary() {
    try {
      summary.value = await getJobSummary({ from: new Date(Date.now() - DAY_MS).toISOString() })
      error.value = null
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load summary'
    }
  }

  async function loadActive() {
    const result = await listJobs({ status: ['Running', 'Queued'] }, 1, 50)
    active.value = result.items
  }

  const running = computed(() => active.value.filter((j) => j.status === 'Running'))
  const queued = computed(() => active.value.filter((j) => j.status === 'Queued'))

  // Summary is an aggregate, so refetch it at most every few seconds instead of on every event.
  const refreshSummary = throttle(() => void loadSummary(), 3000)

  function applyUpdate(job: Job) {
    const rest = active.value.filter((j) => j.id !== job.id)
    active.value = job.status === 'Running' || job.status === 'Queued' ? [job, ...rest] : rest
    refreshSummary()
  }

  function connect() {
    void loadSummary()
    void loadActive()
    const off = onJobUpdated(applyUpdate, () => {
      void loadSummary()
      void loadActive()
    })
    return () => {
      off()
      refreshSummary.cancel()
    }
  }

  return { summary, running, queued, error, connect }
})
