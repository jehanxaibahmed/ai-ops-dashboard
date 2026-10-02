import { describe, expect, it } from 'vitest'
import { buildQuery } from './http'

describe('buildQuery', () => {
  it('skips empty values and repeats arrays', () => {
    expect(buildQuery({ a: 1, b: '', c: undefined, d: ['x', 'y'] })).toBe('?a=1&d=x&d=y')
  })

  it('returns an empty string when nothing is set', () => {
    expect(buildQuery({ a: null })).toBe('')
  })
})
