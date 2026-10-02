import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { parseFilters, serializeFilters, toJobQuery, type FilterControl, type GlobalFilters } from './filters'

/** Global filters read from and written to the current route. */
export function useGlobalFilters() {
  const route = useRoute()
  const router = useRouter()

  const filters = computed(() => parseFilters(route.query))
  const controls = computed(() => (route.meta.filters as FilterControl[] | undefined) ?? [])
  const query = computed(() => toJobQuery(filters.value, controls.value))
  /** Stable key that only changes when an applied filter changes; watch this to reload. */
  const key = computed(() => JSON.stringify({ ...filters.value, controls: controls.value }))

  function update(patch: Partial<GlobalFilters>) {
    void router.replace({ query: serializeFilters({ ...filters.value, ...patch }, route.query) })
  }

  function reset() {
    update({ pipelines: [], models: [], search: '' })
  }

  return { filters, controls, query, key, update, reset }
}
