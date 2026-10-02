import { buildQuery, request } from './http'
import type { CostReport, JobQuery } from './types'

export function getCostReport(query: JobQuery = {}) {
  return request<CostReport>(`/api/analytics/costs${buildQuery({ ...query })}`)
}
