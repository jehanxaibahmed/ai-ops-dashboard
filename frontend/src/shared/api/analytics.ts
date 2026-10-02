import { buildQuery, request } from './http'
import type { AccuracyGroupBy, AccuracyReport, CostReport, JobQuery } from './types'

export function getCostReport(query: JobQuery = {}) {
  return request<CostReport>(`/api/analytics/costs${buildQuery({ ...query })}`)
}

export function getAccuracyReport(query: JobQuery = {}, groupBy: AccuracyGroupBy = 'Pipeline') {
  return request<AccuracyReport>(`/api/analytics/accuracy${buildQuery({ ...query, groupBy })}`)
}
