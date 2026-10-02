import { request } from './http'
import type { SimulationState } from './types'

export function getSimulation() {
  return request<SimulationState>('/api/simulation')
}

export function updateSimulation(patch: Partial<Pick<SimulationState, 'running' | 'speed' | 'failureRate'>>) {
  return request<SimulationState>('/api/simulation', { method: 'PATCH', body: JSON.stringify(patch) })
}
