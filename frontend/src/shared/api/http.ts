export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

type QueryValue = string | number | boolean | null | undefined

export function buildQuery(params: Record<string, QueryValue | QueryValue[]>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    const values = Array.isArray(value) ? value : [value]
    for (const v of values) {
      if (v !== undefined && v !== null && v !== '') search.append(key, String(v))
    }
  }
  const qs = search.toString()
  return qs ? `?${qs}` : ''
}

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    let message = response.statusText
    try {
      const body = await response.json()
      message = body.detail ?? body.title ?? message
    } catch {
      // body was not JSON; keep the status text
    }
    throw new ApiError(response.status, message)
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}
