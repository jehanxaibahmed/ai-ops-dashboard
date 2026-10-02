import { buildQuery, request } from './http'
import type { Job, JobQuery, JobSummary, PagedResult } from './types'

export function listJobs(query: JobQuery, page = 1, pageSize = 50) {
  return request<PagedResult<Job>>(`/api/jobs${buildQuery({ ...query, page, pageSize })}`)
}

export function getJob(id: string) {
  return request<Job>(`/api/jobs/${encodeURIComponent(id)}`)
}

export function getJobSummary(query: JobQuery = {}) {
  return request<JobSummary>(`/api/jobs/summary${buildQuery({ ...query })}`)
}
