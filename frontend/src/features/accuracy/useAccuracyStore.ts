import { defineStore } from 'pinia'
import { ref, shallowRef } from 'vue'
import { getAccuracyReport } from '@/shared/api/analytics'
import type { AccuracyGroupBy, AccuracyReport } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { startOfUtcDaysAgo } from '@/shared/utils/dates'
import { throttle } from '@/shared/utils/throttle'

export const RANGE_OPTIONS = [7, 14] as const
export type RangeDays = (typeof RANGE_OPTIONS)[number]

export const useAccuracyStore = defineStore('accuracy', () => {
  const rangeDays = ref<RangeDays>(14)
  const groupBy = ref<AccuracyGroupBy>('Pipeline')
  const report = shallowRef<AccuracyReport | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  let seq = 0
  async function load() {
    const current = ++seq
    loading.value = true
    try {
      const result = await getAccuracyReport({ from: startOfUtcDaysAgo(rangeDays.value) }, groupBy.value)
      if (current !== seq) return
      report.value = result
      error.value = null
    } catch (e) {
      if (current === seq) error.value = e instanceof Error ? e.message : 'Failed to load accuracy'
    } finally {
      if (current === seq) loading.value = false
    }
  }

  function setRange(days: RangeDays) {
    rangeDays.value = days
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

  return { rangeDays, groupBy, report, loading, error, load, setRange, setGroupBy, connect }
})
