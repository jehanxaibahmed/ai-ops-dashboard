import { defineStore } from 'pinia'
import { ref, shallowRef } from 'vue'
import { getCostReport } from '@/shared/api/analytics'
import type { CostReport, JobQuery } from '@/shared/api/types'
import { onJobUpdated } from '@/shared/realtime/jobsHub'
import { throttle } from '@/shared/utils/throttle'

export const useCostsStore = defineStore('costs', () => {
  const query = ref<JobQuery>({})
  const report = shallowRef<CostReport | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  let seq = 0
  async function load() {
    const current = ++seq
    loading.value = true
    try {
      const result = await getCostReport(query.value)
      if (current !== seq) return
      report.value = result
      error.value = null
    } catch (e) {
      if (current === seq) error.value = e instanceof Error ? e.message : 'Failed to load costs'
    } finally {
      if (current === seq) loading.value = false
    }
  }

  function setQuery(next: JobQuery) {
    query.value = next
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

  return { query, report, loading, error, load, setQuery, connect }
})
