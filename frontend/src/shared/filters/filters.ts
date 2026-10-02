import type { LocationQuery, LocationQueryRaw } from 'vue-router'
import type { JobQuery } from '@/shared/api/types'
import { startOfUtcDaysAgo } from '@/shared/utils/dates'

export const RANGE_PRESETS = ['24h', '7d', '14d'] as const
export type RangePreset = (typeof RANGE_PRESETS)[number]
export const DEFAULT_RANGE: RangePreset = '7d'

export const RANGE_LABELS: Record<RangePreset, string> = {
  '24h': 'Last 24 hours',
  '7d': 'Last 7 days',
  '14d': 'Last 14 days',
}

/** The filters shared by every page. Lives in the URL so views are linkable. */
export interface GlobalFilters {
  range: RangePreset
  pipelines: string[]
  models: string[]
  search: string
}

export type FilterControl = 'range' | 'pipeline' | 'model' | 'search'

function list(value: LocationQuery[string]): string[] {
  const values = Array.isArray(value) ? value : value ? [value] : []
  return values.filter((v): v is string => typeof v === 'string' && v.length > 0)
}

export function parseFilters(query: LocationQuery): GlobalFilters {
  const range = typeof query.range === 'string' && (RANGE_PRESETS as readonly string[]).includes(query.range) ? (query.range as RangePreset) : DEFAULT_RANGE
  return {
    range,
    pipelines: list(query.pipeline),
    models: list(query.model),
    search: typeof query.q === 'string' ? query.q : '',
  }
}

/** Writes filters back into a route query, leaving unrelated keys alone and omitting defaults. */
export function serializeFilters(filters: GlobalFilters, existing: LocationQuery = {}): LocationQueryRaw {
  const { range: _r, pipeline: _p, model: _m, q: _q, ...rest } = existing
  return {
    ...rest,
    ...(filters.range !== DEFAULT_RANGE && { range: filters.range }),
    ...(filters.pipelines.length && { pipeline: filters.pipelines }),
    ...(filters.models.length && { model: filters.models }),
    ...(filters.search && { q: filters.search }),
  }
}

export function rangeStart(range: RangePreset, now: number = Date.now()): string {
  switch (range) {
    case '24h':
      return new Date(now - 24 * 60 * 60 * 1000).toISOString()
    case '7d':
      return startOfUtcDaysAgo(7, now)
    case '14d':
      return startOfUtcDaysAgo(14, now)
  }
}

/** Converts UI filters to the API query. `controls` limits which filters the page applies. */
export function toJobQuery(filters: GlobalFilters, controls: readonly FilterControl[], now: number = Date.now()): JobQuery {
  return {
    from: controls.includes('range') ? rangeStart(filters.range, now) : undefined,
    pipeline: controls.includes('pipeline') && filters.pipelines.length ? filters.pipelines : undefined,
    model: controls.includes('model') && filters.models.length ? filters.models : undefined,
    search: controls.includes('search') && filters.search ? filters.search : undefined,
  }
}
