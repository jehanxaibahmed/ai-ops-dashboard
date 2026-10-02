<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import type { ChartData, ChartOptions, Plugin } from 'chart.js'
import type { CostBreakdownRow } from '@/shared/api/types'
import { ensureChartsRegistered } from '@/shared/components/charts/setup'
import { seriesColor, useChartTheme } from '@/shared/components/charts/useChartTheme'
import { formatCurrency, formatCurrencyTick, formatPercent } from '@/shared/utils/format'

ensureChartsRegistered()

const props = defineProps<{
  rows: CostBreakdownRow[]
  colorIndex: (model: string) => number
  label: (model: string) => string
}>()
const theme = useChartTheme()

const data = computed<ChartData<'bar'>>(() => ({
  labels: props.rows.map((r) => props.label(r.key)),
  datasets: [
    {
      data: props.rows.map((r) => r.costUsd),
      backgroundColor: props.rows.map((r) => seriesColor(theme.value, props.colorIndex(r.key))),
      borderRadius: { topRight: 4, bottomRight: 4 },
      borderSkipped: 'start',
      maxBarThickness: 24,
    },
  ],
}))

// Value at the bar tip, in text ink rather than the series colour.
const tipLabels: Plugin<'bar'> = {
  id: 'tipLabels',
  afterDatasetsDraw(chart) {
    const { ctx } = chart
    const meta = chart.getDatasetMeta(0)
    ctx.save()
    ctx.fillStyle = theme.value.text
    ctx.font = `600 12px ${getComputedStyle(document.documentElement).getPropertyValue('--font-sans')}`
    ctx.textBaseline = 'middle'
    meta.data.forEach((bar, i) => {
      const row = props.rows[i]
      if (!row) return
      ctx.fillText(`${formatCurrency(row.costUsd)} · ${formatPercent(row.share, 0)}`, bar.x + 8, bar.y)
    })
    ctx.restore()
  },
}

const options = computed<ChartOptions<'bar'>>(() => ({
  indexAxis: 'y',
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 250 },
  layout: { padding: { right: 110 } },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: theme.value.surface,
      titleColor: theme.value.text,
      bodyColor: theme.value.text,
      borderColor: theme.value.grid,
      borderWidth: 1,
      padding: 10,
      displayColors: false,
      callbacks: {
        label: (ctx) => {
          const row = props.rows[ctx.dataIndex]!
          return [
            `${formatCurrency(row.costUsd)} (${formatPercent(row.share)})`,
            `${row.jobs} jobs · ${formatCurrency(row.averageCostPerJobUsd ?? 0)} / job`,
          ]
        },
      },
    },
  },
  scales: {
    x: {
      beginAtZero: true,
      grid: { color: theme.value.grid },
      border: { display: false },
      ticks: { color: theme.value.textMuted, callback: (v) => formatCurrencyTick(Number(v)), maxTicksLimit: 5 },
    },
    y: {
      grid: { display: false },
      border: { color: theme.value.grid },
      ticks: { color: theme.value.text },
    },
  },
}))
</script>

<template>
  <div class="chart" role="img" aria-label="Total cost by model. Switch to table view for values.">
    <Bar :data="data" :options="options" :plugins="[tipLabels]" />
  </div>
</template>

<style scoped>
.chart {
  position: relative;
  height: 220px;
}
</style>
