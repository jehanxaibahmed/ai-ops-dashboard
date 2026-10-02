import { describe, expect, it } from 'vitest'
import { formatCurrency, formatDuration, formatPercent, formatRelativeTime } from './format'

describe('format', () => {
  it('keeps precision for sub-dollar amounts', () => {
    expect(formatCurrency(0.0042)).toBe('$0.0042')
    expect(formatCurrency(0.001)).toBe('$0.0010')
    expect(formatCurrency(12.5)).toBe('$12.50')
  })

  it('formats durations by magnitude', () => {
    expect(formatDuration(0.25)).toBe('250 ms')
    expect(formatDuration(12.34)).toBe('12.3 s')
    expect(formatDuration(125)).toBe('2m 5s')
    expect(formatDuration(null)).toBe('—')
  })

  it('formats percentages and handles missing values', () => {
    expect(formatPercent(0.8833)).toBe('88.3%')
    expect(formatPercent(null)).toBe('—')
  })

  it('formats relative times', () => {
    const now = Date.parse('2026-01-01T12:00:00Z')
    expect(formatRelativeTime('2026-01-01T11:59:58Z', now)).toBe('just now')
    expect(formatRelativeTime('2026-01-01T11:58:00Z', now)).toBe('2m ago')
    expect(formatRelativeTime('2025-12-30T12:00:00Z', now)).toBe('2d ago')
  })
})
