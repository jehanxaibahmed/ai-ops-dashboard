import type { Job } from '@/shared/api/types'

export interface MergeOptions {
  /** The job matches the active filter. */
  matches: boolean
  /** New jobs are only inserted on the first page, since the list is newest first. */
  isFirstPage: boolean
  pageSize: number
}

export interface MergeResult {
  items: Job[]
  added: boolean
}

/**
 * Applies one live update to the visible page. Jobs already on screen are updated in place,
 * even if they no longer match the filter, so users see the transition instead of a row
 * vanishing. New matching jobs are prepended on page one.
 */
export function mergeJobUpdate(items: Job[], job: Job, opts: MergeOptions): MergeResult {
  const index = items.findIndex((j) => j.id === job.id)
  if (index >= 0) {
    // Ignore stale updates that arrive out of order.
    if (Date.parse(items[index]!.updatedAt) > Date.parse(job.updatedAt)) return { items, added: false }
    const next = items.slice()
    next[index] = job
    return { items: next, added: false }
  }

  if (opts.matches && opts.isFirstPage) {
    return { items: [job, ...items].slice(0, opts.pageSize), added: true }
  }

  return { items, added: false }
}
