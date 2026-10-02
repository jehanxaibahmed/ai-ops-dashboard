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
  canRetry: boolean
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
  fields: string[]
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

export interface RetryResult {
  retried: Job[]
  skipped: { jobId: string; reason: string }[]
}

export interface FailureCodeStat {
  code: string
  sampleMessage: string
  isTransient: boolean
  count: number
  lastSeenAt: string
}

export interface FailurePipelineStat {
  pipelineId: string
  failed: number
  finished: number
  failureRate: number
}

export interface FailureBreakdown {
  totalFailed: number
  retryable: number
  byCode: FailureCodeStat[]
  byPipeline: FailurePipelineStat[]
}

export interface CostTotals {
  totalCostUsd: number
  jobs: number
  inputTokens: number
  outputTokens: number
  averageCostPerJobUsd: number | null
  failedCostUsd: number
}

export interface CostBreakdownRow {
  key: string
  jobs: number
  inputTokens: number
  outputTokens: number
  costUsd: number
  averageCostPerJobUsd: number | null
  share: number
}

export interface DailyCost {
  /** UTC day, `YYYY-MM-DD`. */
  date: string
  totalCostUsd: number
  byModel: Record<string, number>
}

export interface CostReport {
  totals: CostTotals
  byModel: CostBreakdownRow[]
  byPipeline: CostBreakdownRow[]
  models: string[]
  daily: DailyCost[]
}

export type AccuracyGroupBy = 'Pipeline' | 'Model'

export interface AccuracyRow {
  key: string
  accuracy: number
  evaluations: number
  /** Last 3 days minus the rest of the range, in accuracy points. */
  recentChange: number | null
}

export interface DailyAccuracy {
  date: string
  accuracy: Record<string, number | null>
  evaluations: Record<string, number>
}

export interface FieldErrorStat {
  pipelineId: string
  field: string
  misses: number
  checked: number
  errorRate: number
}

export interface AccuracyReport {
  groupBy: AccuracyGroupBy
  overall: { accuracy: number | null; perfectRate: number | null; evaluations: number; fieldsChecked: number }
  keys: string[]
  rows: AccuracyRow[]
  daily: DailyAccuracy[]
  worstFields: FieldErrorStat[]
}
