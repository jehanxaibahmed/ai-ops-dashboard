import { defineStore } from 'pinia'
import { ref, shallowRef } from 'vue'
import { getCostReport } from '@/shared/api/analytics'
import type { CostReport } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { startOfUtcDaysAgo } from '@/shared/utils/dates'
import { throttle } from '@/shared/utils/throttle'

export const RANGE_OPTIONS = [7, 14] as const
export type RangeDays = (typeof RANGE_OPTIONS)[number]

export const useCostsStore = defineStore('costs', () => {
  const rangeDays = ref<RangeDays>(14)
  const report = shallowRef<CostReport | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  let seq = 0
  async function load() {
    const current = ++seq
    loading.value = true
    try {
      const result = await getCostReport({ from: startOfUtcDaysAgo(rangeDays.value) })
      if (current !== seq) return
      report.value = result
      error.value = null
    } catch (e) {
      if (current === seq) error.value = e instanceof Error ? e.message : 'Failed to load costs'
    } finally {
      if (current === seq) loading.value = false
    }
  }

  function setRange(days: RangeDays) {
    rangeDays.value = days
    return load()
  }

  // Spend changes with every running job; refreshing every 10 s is plenty for a cost view.
  const refresh = throttle(() => void load(), 10_000)

  function connect() {
    const off = onJobUpdated(() => refresh(), () => void load())
    return () => {
      off()
      refresh.cancel()
    }
  }

  return { rangeDays, report, loading, error, load, setRange, connect }
})
