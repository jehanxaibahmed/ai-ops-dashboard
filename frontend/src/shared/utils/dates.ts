const DAY_MS = 24 * 60 * 60 * 1000

/** Start of the UTC day `days - 1` days ago, so "7 days" means today plus the six before it. */
export function startOfUtcDaysAgo(days: number, now: number = Date.now()): string {
  const d = new Date(now)
  const start = Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate()) - (days - 1) * DAY_MS
  return new Date(start).toISOString()
}

/** `2026-10-02` → `Oct 2`, read as a UTC calendar day. */
export function formatDay(isoDate: string): string {
  return new Date(`${isoDate}T00:00:00Z`).toLocaleDateString(undefined, { month: 'short', day: 'numeric', timeZone: 'UTC' })
}
