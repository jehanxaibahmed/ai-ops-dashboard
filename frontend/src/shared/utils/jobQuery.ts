import type { Job, JobQuery } from '@/shared/api/types'

/** Client-side mirror of the backend JobFilter, used to decide if a live update belongs in a view. */
export function matchesQuery(job: Job, query: JobQuery): boolean {
  if (query.status?.length && !query.status.includes(job.status)) return false
  if (query.pipeline?.length && !query.pipeline.includes(job.pipelineId)) return false
  if (query.model?.length && !query.model.includes(job.model)) return false
  const created = Date.parse(job.createdAt)
  if (query.from && created < Date.parse(query.from)) return false
  if (query.to && created >= Date.parse(query.to)) return false
  if (query.search) {
    const s = query.search.toLowerCase()
    if (!job.documentName.toLowerCase().includes(s) && !job.id.toLowerCase().startsWith(s)) return false
  }
  return true
}
