import { describe, expect, it } from 'vitest'
import type { Job } from '@/shared/api/types'
import { mergeJobUpdate } from './liveMerge'

function job(id: string, patch: Partial<Job> = {}): Job {
  return {
    id,
    pipelineId: 'invoice-extraction',
    model: 'claude-sonnet',
    documentName: `${id}.pdf`,
    status: 'Running',
    progress: 10,
    attempt: 1,
    createdAt: '2026-01-01T00:00:00Z',
    startedAt: null,
    completedAt: null,
    updatedAt: '2026-01-01T00:00:01Z',
    durationSeconds: null,
    inputTokens: 0,
    outputTokens: 0,
    costUsd: 0,
    failure: null,
    ...patch,
  }
}

const opts = { matches: true, isFirstPage: true, pageSize: 3 }

describe('mergeJobUpdate', () => {
  it('updates a visible job in place', () => {
    const items = [job('a'), job('b')]
    const updated = job('b', { progress: 80, updatedAt: '2026-01-01T00:00:05Z' })

    const result = mergeJobUpdate(items, updated, opts)

    expect(result.added).toBe(false)
    expect(result.items[1]!.progress).toBe(80)
  })

  it('keeps a visible job even when it stops matching', () => {
    const items = [job('a')]
    const result = mergeJobUpdate(items, job('a', { status: 'Failed', updatedAt: '2026-01-01T00:00:09Z' }), {
      ...opts,
      matches: false,
    })

    expect(result.items[0]!.status).toBe('Failed')
  })

  it('ignores out-of-order updates', () => {
    const items = [job('a', { progress: 50, updatedAt: '2026-01-01T00:00:10Z' })]
    const result = mergeJobUpdate(items, job('a', { progress: 20, updatedAt: '2026-01-01T00:00:05Z' }), opts)

    expect(result.items[0]!.progress).toBe(50)
  })

  it('prepends new matching jobs on page one and trims to page size', () => {
    const items = [job('a'), job('b'), job('c')]
    const result = mergeJobUpdate(items, job('d'), opts)

    expect(result.added).toBe(true)
    expect(result.items.map((j) => j.id)).toEqual(['d', 'a', 'b'])
  })

  it('does not insert new jobs on later pages or when filtered out', () => {
    const items = [job('a')]

    expect(mergeJobUpdate(items, job('x'), { ...opts, isFirstPage: false }).items).toBe(items)
    expect(mergeJobUpdate(items, job('y'), { ...opts, matches: false }).items).toBe(items)
  })
})
