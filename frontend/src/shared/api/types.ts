// Mirrors the backend DTOs in AiOps.Application. Keep in sync when contracts change.

export const JOB_STATUSES = ['Queued', 'Running', 'Succeeded', 'Failed'] as const
export type JobStatus = (typeof JOB_STATUSES)[number]

export interface JobFailure {
  code: string
  message: string
  isTransient: boolean
}

export interface Job {
  id: string
  pipelineId: string
  model: string
  documentName: string
  status: JobStatus
  progress: number
  attempt: number
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  updatedAt: string
  durationSeconds: number | null
  inputTokens: number
  outputTokens: number
  costUsd: number
  failure: JobFailure | null
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface JobSummary {
  total: number
  queued: number
  running: number
  succeeded: number
  failed: number
  successRate: number | null
  averageDurationSeconds: number | null
  totalCostUsd: number
}

export interface Pipeline {
  id: string
  name: string
  description: string
  models: string[]
}

export interface Model {
  id: string
  displayName: string
  provider: string
  inputPerMillion: number
  outputPerMillion: number
}

export interface Catalog {
  pipelines: Pipeline[]
  models: Model[]
}

/** Filter accepted by every job and analytics endpoint. */
export interface JobQuery {
  status?: JobStatus[]
  pipeline?: string[]
  model?: string[]
  from?: string
  to?: string
  search?: string
}
