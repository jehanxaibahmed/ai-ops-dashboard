/** Runs `fn` at most once per `ms`, always including a trailing call. */
export function throttle(fn: () => void, ms: number): (() => void) & { cancel: () => void } {
  let timer: ReturnType<typeof setTimeout> | null = null
  let last = 0

  const run = () => {
    last = Date.now()
    timer = null
    fn()
  }

  const throttled = () => {
    if (timer) return
    const wait = Math.max(0, ms - (Date.now() - last))
    timer = setTimeout(run, wait)
  }
  throttled.cancel = () => {
    if (timer) clearTimeout(timer)
    timer = null
  }
  return throttled
}
