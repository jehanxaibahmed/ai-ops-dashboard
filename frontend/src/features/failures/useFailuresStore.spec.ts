import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { makeJob } from '@/test/factories'
import { useFailuresStore } from './useFailuresStore'

describe('useFailuresStore', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('only offers retryable transient jobs for bulk transient retry', () => {
    const store = useFailuresStore()
    store.items = [
      makeJob('a'),
      makeJob('b', { failure: { code: 'invalid_document', message: 'x', isTransient: false } }),
      makeJob('c', { canRetry: false }),
    ]

    expect(store.transientRetryable.map((j) => j.id)).toEqual(['a'])
  })

  it('sends the error code to the API and clears selection when it changes', () => {
    const fetchsimulator = vi.fn((_input: string) => new Promise<Response>(() => {}))
    vi.stubGlobal('fetch', fetchsimulator)
    const store = useFailuresStore()
    store.toggle('a')

    void store.setCode('rate_limited')

    expect(store.selection.size).toBe(0)
    expect(String(fetchsimulator.simulator.calls[0]?.[0])).toContain('failureCode=rate_limited')
    vi.unstubAllGlobals()
  })

  it('toggleAll selects all, then clears when everything is selected', () => {
    const store = useFailuresStore()
    store.toggleAll(['a', 'b'])
    expect([...store.selection]).toEqual(['a', 'b'])

    store.toggleAll(['a', 'b'])
    expect(store.selection.size).toBe(0)
  })

  it('a retried job stays visible with its new status', () => {
    const store = useFailuresStore()
    store.items = [makeJob('a')]

    store.applyUpdate(makeJob('a', { status: 'Queued', attempt: 2, failure: null, updatedAt: '2026-01-01T00:01:00Z' }))

    expect(store.items[0]!.status).toBe('Queued')
    expect(store.selectedRetryable).toEqual([])
  })
})
