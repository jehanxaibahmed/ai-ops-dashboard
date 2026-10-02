import type { Job } from '@/shared/api/types'

export function makeJob(id: string, patch: Partial<Job> = {}): Job {
  return {
    id,
    pipelineId: 'invoice-extraction',
    model: 'claude-sonnet',
    documentName: `${id}.pdf`,
    status: 'Failed',
    progress: 99,
    attempt: 1,
    createdAt: '2026-01-01T00:00:00Z',
    startedAt: '2026-01-01T00:00:01Z',
    completedAt: '2026-01-01T00:00:05Z',
    updatedAt: '2026-01-01T00:00:05Z',
    durationSeconds: 4,
    inputTokens: 1000,
    outputTokens: 100,
    costUsd: 0.01,
    failure: { code: 'timeout', message: 'timed out', isTransient: true },
    canRetry: true,
    ...patch,
  }
}
