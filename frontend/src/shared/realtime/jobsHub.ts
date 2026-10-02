import { HubConnectionBuilder, HubConnectionState, LogLevel, type HubConnection } from '@microsoft/signalr'
import { readonly, ref } from 'vue'
import type { Job, SimulationState } from '@/shared/api/types'

export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting'

type JobListener = (job: Job) => void
type ReconnectListener = () => void

const status = ref<ConnectionStatus>('disconnected')
const jobListeners = new Set<JobListener>()
const simulationListeners = new Set<(state: SimulationState) => void>()
const reconnectListeners = new Set<ReconnectListener>()
let connection: HubConnection | null = null
let starting: Promise<void> | null = null

function build(): HubConnection {
  const conn = new HubConnectionBuilder()
    .withUrl('/hubs/jobs')
    .withAutomaticReconnect([0, 1000, 3000, 5000, 10000])
    .configureLogging(LogLevel.Warning)
    .build()

  conn.on('JobUpdated', (job: Job) => jobListeners.forEach((l) => l(job)))
  conn.on('SimulationChanged', (state: SimulationState) => simulationListeners.forEach((l) => l(state)))
  conn.onreconnecting(() => (status.value = 'reconnecting'))
  conn.onreconnected(() => {
    status.value = 'connected'
    // Updates sent while we were offline are lost, so listeners should refetch.
    reconnectListeners.forEach((l) => l())
  })
  conn.onclose(() => (status.value = 'disconnected'))
  return conn
}

async function start(): Promise<void> {
  connection ??= build()
  if (connection.state !== HubConnectionState.Disconnected) return starting ?? Promise.resolve()

  status.value = 'connecting'
  starting = connection
    .start()
    .then(() => {
      status.value = 'connected'
    })
    .catch(() => {
      status.value = 'disconnected'
      // Retry the initial connection; automatic reconnect only covers drops after connecting.
      setTimeout(() => void start(), 3000)
    })
    .finally(() => {
      starting = null
    })
  return starting
}

/**
 * Subscribe to live job updates. Starts the shared connection on first use.
 * Returns an unsubscribe function.
 */
export function onJobUpdated(listener: JobListener, onReconnected?: ReconnectListener): () => void {
  jobListeners.add(listener)
  if (onReconnected) reconnectListeners.add(onReconnected)
  void start()
  return () => {
    jobListeners.delete(listener)
    if (onReconnected) reconnectListeners.delete(onReconnected)
  }
}

/** Subscribe to demo-mode changes made from any tab. */
export function onSimulationChanged(listener: (state: SimulationState) => void): () => void {
  simulationListeners.add(listener)
  void start()
  return () => simulationListeners.delete(listener)
}

export function useJobsHub() {
  void start()
  return { status: readonly(status) }
}
