import { onBeforeUnmount, onMounted, ref } from 'vue'

/** A timestamp that refreshes on an interval, for "2m ago" style labels. */
export function useNow(intervalMs = 5000) {
  const now = ref(Date.now())
  let timer: ReturnType<typeof setInterval> | undefined
  onMounted(() => (timer = setInterval(() => (now.value = Date.now()), intervalMs)))
  onBeforeUnmount(() => clearInterval(timer))
  return now
}
