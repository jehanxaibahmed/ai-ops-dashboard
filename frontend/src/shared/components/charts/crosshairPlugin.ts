import type { Plugin } from 'chart.js'

/** Draws a vertical hairline at the active tooltip position, so readers aim at a date, not a line. */
export function crosshairPlugin(color: () => string): Plugin<'line'> {
  return {
    id: 'crosshair',
    afterDatasetsDraw(chart) {
      const active = chart.tooltip?.getActiveElements()
      if (!active?.length) return
      const x = active[0]!.element.x
      const { top, bottom } = chart.chartArea
      const ctx = chart.ctx
      ctx.save()
      ctx.strokeStyle = color()
      ctx.lineWidth = 1
      ctx.beginPath()
      ctx.moveTo(x, top)
      ctx.lineTo(x, bottom)
      ctx.stroke()
      ctx.restore()
    },
  }
}
