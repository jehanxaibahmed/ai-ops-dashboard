<script setup lang="ts">
import { computed } from 'vue'
import { Line } from 'vue-chartjs'
import type { ChartData, ChartOptions } from 'chart.js'
import type { DailyAccuracy } from '@/shared/api/types'
import LegendList from '@/shared/components/charts/LegendList.vue'
import { crosshairPlugin } from '@/shared/components/charts/crosshairPlugin'
import { ensureChartsRegistered } from '@/shared/components/charts/setup'
import { seriesColor, useChartTheme } from '@/shared/components/charts/useChartTheme'
import { formatDay } from '@/shared/utils/dates'
import { formatPercent } from '@/shared/utils/format'

ensureChartsRegistered()

const props = defineProps<{
  daily: DailyAccuracy[]
  keys: string[]
  colorIndex: (key: string) => number
  label: (key: string) => string
}>()
const theme = useChartTheme()

const legend = computed(() =>
  props.keys.map((k) => ({ label: props.label(k), color: seriesColor(theme.value, props.colorIndex(k)) })),
)

/** Index of the last day that has a value, for the end-dot marker. */
function lastIndex(key: string) {
  for (let i = props.daily.length - 1; i >= 0; i--) if (props.daily[i]!.accuracy[key] != null) return i
  return -1
}

const data = computed<ChartData<'line'>>(() => ({
  labels: props.daily.map((d) => formatDay(d.date)),
  datasets: props.keys.map((key) => {
    const color = seriesColor(theme.value, props.colorIndex(key))
    const last = lastIndex(key)
    return {
      label: props.label(key),
      data: props.daily.map((d) => d.accuracy[key] ?? null),
      borderColor: color,
      backgroundColor: color,
      borderWidth: 2,
      borderJoinStyle: 'round' as const,
      borderCapStyle: 'round' as const,
      tension: 0,
      spanGaps: false,
      // End-dot only, with a 2px surface ring so it stays legible where lines cross.
      pointRadius: props.daily.map((_, i) => (i === last ? 4 : 0)),
      pointHoverRadius: 5,
      pointBorderColor: theme.value.surface,
      pointBorderWidth: 2,
      pointHitRadius: 12,
    }
  }),
}))

const options = computed<ChartOptions<'line'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 250 },
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: theme.value.surface,
      titleColor: theme.value.text,
      bodyColor: theme.value.text,
      borderColor: theme.value.grid,
      borderWidth: 1,
      padding: 10,
      boxWidth: 12,
      boxHeight: 2,
      itemSort: (a, b) => Number(b.raw ?? -1) - Number(a.raw ?? -1),
      callbacks: {
        label: (ctx) => {
          const day = props.daily[ctx.dataIndex]!
          const key = props.keys[ctx.datasetIndex]!
          const n = day.evaluations[key] ?? 0
          return ` ${ctx.raw == null ? 'no data' : formatPercent(Number(ctx.raw))}  ${ctx.dataset.label} (${n})`
        },
      },
    },
  },
  scales: {
    x: {
      grid: { display: false },
      border: { color: theme.value.grid },
      ticks: { color: theme.value.textMuted, maxRotation: 0, autoSkipPadding: 12 },
    },
    y: {
      grace: '5%',
      max: 1,
      grid: { color: theme.value.grid },
      border: { display: false },
      ticks: { color: theme.value.textMuted, callback: (v) => formatPercent(Number(v), 0), maxTicksLimit: 6 },
    },
  },
}))

const plugins = [crosshairPlugin(() => theme.value.axis)]
</script>

<template>
  <LegendList :items="legend" shape="line" />
  <div class="chart" role="img" aria-label="Daily accuracy trend per series. Switch to table view for values.">
    <Line :data="data" :options="options" :plugins="plugins" />
  </div>
</template>

<style scoped>
.chart {
  position: relative;
  height: 300px;
}
</style>
