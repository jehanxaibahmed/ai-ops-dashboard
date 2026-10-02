import { describe, expect, it } from 'vitest'
import { DEFAULT_RANGE, parseFilters, serializeFilters, toJobQuery } from './filters'

describe('filters', () => {
  it('parses repeated and single query values and falls back to defaults', () => {
    const f = parseFilters({ pipeline: ['a', 'b'], model: 'm', range: 'bogus' })
    expect(f).toEqual({ range: DEFAULT_RANGE, pipelines: ['a', 'b'], models: ['m'], search: '' })
  })

  it('round-trips through the URL and keeps unrelated keys', () => {
    const f = { range: '24h' as const, pipelines: ['a'], models: [], search: 'inv' }
    const q = serializeFilters(f, { status: 'Failed', pipeline: 'old' })
    expect(q).toEqual({ status: 'Failed', range: '24h', pipeline: ['a'], q: 'inv' })
    expect(parseFilters(q as never)).toEqual(f)
  })

  it('omits the default range from the URL', () => {
    expect(serializeFilters({ range: DEFAULT_RANGE, pipelines: [], models: [], search: '' })).toEqual({})
  })

  it('only applies the controls a page supports', () => {
    const now = Date.parse('2026-10-02T12:00:00Z')
    const q = toJobQuery({ range: '24h', pipelines: ['a'], models: ['m'], search: 'x' }, ['range', 'model'], now)
    expect(q).toEqual({ from: '2026-10-01T12:00:00.000Z', pipeline: undefined, model: ['m'], search: undefined })
  })
})
