import { describe, expect, it } from 'vitest'
import { formatDay, startOfUtcDaysAgo } from './dates'

describe('dates', () => {
  it('counts today as the first day of the range', () => {
    const now = Date.parse('2026-10-02T15:30:00Z')
    expect(startOfUtcDaysAgo(1, now)).toBe('2026-10-02T00:00:00.000Z')
    expect(startOfUtcDaysAgo(7, now)).toBe('2026-09-26T00:00:00.000Z')
  })

  it('formats a UTC day without shifting it by the local timezone', () => {
    expect(formatDay('2026-10-02')).toMatch(/Oct 2/)
  })
})
