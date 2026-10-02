import { onBeforeUnmount, onMounted, ref } from 'vue'

export interface ChartTheme {
  series: string[]
  surface: string
  text: string
  textMuted: string
  grid: string
  axis: string
}

const SERIES_SLOTS = 4

function read(): ChartTheme {
  const css = getComputedStyle(document.documentElement)
  const v = (name: string) => css.getPropertyValue(name).trim()
  return {
    series: Array.from({ length: SERIES_SLOTS }, (_, i) => v(`--series-${i + 1}`) || '#888'),
    surface: v('--color-surface'),
    text: v('--color-text'),
    textMuted: v('--color-text-muted'),
    grid: v('--chart-grid'),
    axis: v('--chart-axis'),
  }
}

/**
 * Chart colours from CSS tokens. Re-reads when the OS colour scheme changes,
 * so dark mode uses its own validated steps instead of a filter or flip.
 */
export function useChartTheme() {
  const theme = ref<ChartTheme>(read())
  let media: MediaQueryList | undefined
  const update = () => (theme.value = read())

  onMounted(() => {
    media = window.matchMedia('(prefers-color-scheme: dark)')
    media.addEventListener('change', update)
    update()
  })
  onBeforeUnmount(() => media?.removeEventListener('change', update))

  return theme
}

/**
 * Colour follows the entity, never its rank: index by the entity's fixed position
 * (catalog order), so filtering a series out never repaints the others.
 */
export function seriesColor(theme: ChartTheme, index: number): string {
  return theme.series[index % theme.series.length] ?? theme.axis
}
