import { defineStore } from 'pinia'
import { ref, shallowRef } from 'vue'
import { getAccuracyReport } from '@/shared/api/analytics'
import type { AccuracyGroupBy, AccuracyReport, JobQuery } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { throttle } from '@/shared/utils/throttle'

export const useAccuracyStore = defineStore('accuracy', () => {
  const query = ref<JobQuery>({})
  const groupBy = ref<AccuracyGroupBy>('Pipeline')
  const report = shallowRef<AccuracyReport | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  let seq = 0
  async function load() {
    const current = ++seq
    loading.value = true
    try {
      const result = await getAccuracyReport(query.value, groupBy.value)
      if (current !== seq) return
      report.value = result
      error.value = null
    } catch (e) {
      if (current === seq) error.value = e instanceof Error ? e.message : 'Failed to load accuracy'
    } finally {
      if (current === seq) loading.value = false
    }
  }

  function setQuery(next: JobQuery) {
    query.value = next
    return load()
  }

  function setGroupBy(value: AccuracyGroupBy) {
    groupBy.value = value
    return load()
  }

  // Only succeeded jobs produce evaluations.
  const refresh = throttle(() => void load(), 15_000)

  function connect() {
    const off = onJobUpdated((job) => job.status === 'Succeeded' && refresh(), () => void load())
    return () => {
      off()
      refresh.cancel()
    }
  }

  return { query, groupBy, report, loading, error, load, setQuery, setGroupBy, connect }
})
