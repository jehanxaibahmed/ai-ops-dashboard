import {
  BarElement,
  CategoryScale,
  Chart,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Tooltip,
} from 'chart.js'

let registered = false

/** Registers only the Chart.js pieces this app uses, so the bundle stays small. */
export function ensureChartsRegistered() {
  if (registered) return
  Chart.register(BarElement, CategoryScale, LinearScale, LineElement, PointElement, Filler, Tooltip, Legend)
  Chart.defaults.font.family = getComputedStyle(document.documentElement).getPropertyValue('--font-sans') || 'system-ui'
  Chart.defaults.font.size = 12
  registered = true
}
