const usd = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 2 })
const usdPrecise = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 4,
  maximumFractionDigits: 4,
})
const compact = new Intl.NumberFormat('en-US', { notation: 'compact', maximumFractionDigits: 1 })
const integer = new Intl.NumberFormat('en-US')

/** Small amounts keep more decimals so a $0.0042 job does not show as $0.00. */
export function formatCurrency(value: number): string {
  return Math.abs(value) > 0 && Math.abs(value) < 1 ? usdPrecise.format(value) : usd.format(value)
}

/** Axis ticks: clean round numbers, never the 4-decimal precision used for single values. */
export function formatCurrencyTick(value: number): string {
  return usd.format(value)
}

export function formatNumber(value: number): string {
  return integer.format(value)
}

export function formatCompact(value: number): string {
  return compact.format(value)
}

export function formatPercent(value: number | null | undefined, digits = 1): string {
  if (value === null || value === undefined) return '—'
  return `${(value * 100).toFixed(digits)}%`
}

export function formatDuration(seconds: number | null | undefined): string {
  if (seconds === null || seconds === undefined) return '—'
  if (seconds < 1) return `${Math.round(seconds * 1000)} ms`
  if (seconds < 60) return `${seconds.toFixed(1)} s`
  const m = Math.floor(seconds / 60)
  const s = Math.round(seconds % 60)
  return `${m}m ${s}s`
}

export function formatRelativeTime(iso: string, now: number = Date.now()): string {
  const diff = Math.round((now - new Date(iso).getTime()) / 1000)
  if (diff < 5) return 'just now'
  if (diff < 60) return `${diff}s ago`
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`
  return `${Math.floor(diff / 86400)}d ago`
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

export function shortId(id: string): string {
  return id.slice(0, 8)
}
