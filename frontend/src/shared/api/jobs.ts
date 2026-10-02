import { buildQuery, request } from './http'
import type { FailureBreakdown, Job, JobQuery, JobSummary, PagedResult, RetryResult } from './types'

export function listJobs(query: JobQuery, page = 1, pageSize = 50) {
  return request<PagedResult<Job>>(`/api/jobs${buildQuery({ ...query, page, pageSize })}`)
}

export function getJob(id: string) {
  return request<Job>(`/api/jobs/${encodeURIComponent(id)}`)
}

export function getJobSummary(query: JobQuery = {}) {
  return request<JobSummary>(`/api/jobs/summary${buildQuery({ ...query })}`)
}

export function retryJob(id: string) {
  return request<Job>(`/api/jobs/${encodeURIComponent(id)}/retry`, { method: 'POST' })
}

export function retryJobs(jobIds: string[]) {
  return request<RetryResult>('/api/jobs/retry', { method: 'POST', body: JSON.stringify({ jobIds }) })
}

export function getFailureBreakdown(query: JobQuery = {}) {
  return request<FailureBreakdown>(`/api/failures/breakdown${buildQuery({ ...query })}`)
}
