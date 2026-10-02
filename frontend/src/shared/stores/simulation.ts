import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getSimulation, updateSimulation } from '@/shared/api/simulation'
import type { SimulationState } from '@/shared/api/types'
import { onSimulationChanged } from '@/shared/realtime/jobsHub'
import { useToastStore } from './toasts'

export const SPEEDS = [0.5, 1, 2, 5] as const
export const FAILURE_RATES = [0.05, 0.12, 0.3] as const

/** Demo-mode state, shared across tabs through SignalR. */
export const useSimulationStore = defineStore('simulation', () => {
  const state = ref<SimulationState | null>(null)
  const saving = ref(false)
  let subscribed = false

  async function init() {
    if (!subscribed) {
      subscribed = true
      onSimulationChanged((s) => (state.value = s))
    }
    try {
      state.value = await getSimulation()
    } catch {
      // backend down; the connection indicator already says so
    }
  }

  async function update(patch: Partial<Pick<SimulationState, 'running' | 'speed' | 'failureRate'>>) {
    saving.value = true
    try {
      state.value = await updateSimulation(patch)
    } catch (e) {
      useToastStore().push(e instanceof Error ? e.message : 'Could not update demo mode', 'danger')
    } finally {
      saving.value = false
    }
  }

  return { state, saving, init, update }
})
