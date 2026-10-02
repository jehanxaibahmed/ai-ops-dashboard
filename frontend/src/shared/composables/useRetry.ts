import { ref } from 'vue'
import { retryJob, retryJobs } from '@/shared/api/jobs'
import type { Job } from '@/shared/api/types'
import { useToastStore } from '@/shared/stores/toasts'

/** Retry actions with loading state and user feedback. */
export function useRetry(onRetried?: (jobs: Job[]) => void) {
  const toasts = useToastStore()
  const pending = ref(new Set<string>())
  const bulkPending = ref(false)

  async function retryOne(id: string) {
    pending.value = new Set(pending.value).add(id)
    try {
      const job = await retryJob(id)
      onRetried?.([job])
      toasts.push(`Retrying ${job.documentName} (attempt ${job.attempt})`, 'success')
    } catch (e) {
      toasts.push(e instanceof Error ? e.message : 'Retry failed', 'danger')
    } finally {
      const next = new Set(pending.value)
      next.delete(id)
      pending.value = next
    }
  }

  async function retryMany(ids: string[]) {
    if (ids.length === 0) return
    bulkPending.value = true
    try {
      const result = await retryJobs(ids)
      onRetried?.(result.retried)
      const skipped = result.skipped.length ? `, ${result.skipped.length} skipped` : ''
      toasts.push(`Retrying ${result.retried.length} jobs${skipped}`, result.retried.length ? 'success' : 'info')
    } catch (e) {
      toasts.push(e instanceof Error ? e.message : 'Retry failed', 'danger')
    } finally {
      bulkPending.value = false
    }
  }

  return { pending, bulkPending, retryOne, retryMany }
}
