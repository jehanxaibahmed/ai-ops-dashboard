<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import type { ChartData, ChartOptions } from 'chart.js'
import type { DailyCost } from '@/shared/api/types'
import LegendList from '@/shared/components/charts/LegendList.vue'
import { ensureChartsRegistered } from '@/shared/components/charts/setup'
import { seriesColor, useChartTheme } from '@/shared/components/charts/useChartTheme'
import { formatDay } from '@/shared/utils/dates'
import { formatCurrency, formatCurrencyTick } from '@/shared/utils/format'

ensureChartsRegistered()

const props = defineProps<{
  daily: DailyCost[]
  models: string[]
  colorIndex: (model: string) => number
  label: (model: string) => string
}>()
const theme = useChartTheme()

const legend = computed(() =>
  props.models.map((m) => ({ label: props.label(m), color: seriesColor(theme.value, props.colorIndex(m)) })),
)

const data = computed<ChartData<'bar'>>(() => ({
  labels: props.daily.map((d) => formatDay(d.date)),
  datasets: props.models.map((model, i) => {
    const isTop = i === props.models.length - 1
    return {
      label: props.label(model),
      data: props.daily.map((d) => d.byModel[model] ?? 0),
      backgroundColor: seriesColor(theme.value, props.colorIndex(model)),
      // 2px surface gap between stacked segments; rounded data-end only on the top segment.
      borderColor: theme.value.surface,
      borderWidth: i === 0 ? 0 : { top: 0, bottom: 2, left: 0, right: 0 },
      borderSkipped: false,
      borderRadius: isTop ? { topLeft: 4, topRight: 4 } : 0,
      maxBarThickness: 24,
      stack: 'cost',
    }
  }),
}))

const options = computed<ChartOptions<'bar'>>(() => ({
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
      footerColor: theme.value.text,
      borderColor: theme.value.grid,
      borderWidth: 1,
      padding: 10,
      boxWidth: 12,
      boxHeight: 2,
      itemSort: (a, b) => b.datasetIndex - a.datasetIndex,
      callbacks: {
        label: (ctx) => ` ${formatCurrency(Number(ctx.raw))}  ${ctx.dataset.label}`,
        footer: (items) => `Total ${formatCurrency(items.reduce((sum, i) => sum + Number(i.raw), 0))}`,
      },
    },
  },
  scales: {
    x: {
      stacked: true,
      grid: { display: false },
      border: { color: theme.value.grid },
      ticks: { color: theme.value.textMuted, maxRotation: 0, autoSkipPadding: 12 },
    },
    y: {
      stacked: true,
      beginAtZero: true,
      grid: { color: theme.value.grid },
      border: { display: false },
      ticks: { color: theme.value.textMuted, callback: (v) => formatCurrencyTick(Number(v)), maxTicksLimit: 6 },
    },
  },
}))
</script>

<template>
  <LegendList :items="legend" />
  <div class="chart" role="img" aria-label="Daily cost by model, stacked columns. Switch to table view for values.">
    <Bar :data="data" :options="options" />
  </div>
</template>

<style scoped>
.chart {
  position: relative;
  height: 280px;
}
</style>
