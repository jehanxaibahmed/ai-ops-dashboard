<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { RANGE_LABELS, RANGE_PRESETS } from '@/shared/filters/filters'
import { useGlobalFilters } from '@/shared/filters/useGlobalFilters'
import { useCatalogStore } from '@/shared/stores/catalog'
import MultiSelect from './MultiSelect.vue'

const { filters, controls, update, reset } = useGlobalFilters()
const catalog = useCatalogStore()
void catalog.ensureLoaded()

const pipelineOptions = computed(() => catalog.pipelines.map((p) => ({ value: p.id, label: p.name })))
const modelOptions = computed(() => catalog.models.map((m) => ({ value: m.id, label: m.displayName })))

const pipelines = computed({ get: () => filters.value.pipelines, set: (v) => update({ pipelines: v }) })
const models = computed({ get: () => filters.value.models, set: (v) => update({ models: v }) })

// Debounce search so typing doesn't fire a request per keystroke.
const search = ref(filters.value.search)
let timer: ReturnType<typeof setTimeout> | undefined
watch(search, (value) => {
  clearTimeout(timer)
  timer = setTimeout(() => update({ search: value.trim() }), 300)
})
watch(
  () => filters.value.search,
  (value) => {
    if (value !== search.value.trim()) search.value = value
  },
)

const hasNarrowing = computed(
  () => filters.value.pipelines.length > 0 || filters.value.models.length > 0 || filters.value.search !== '',
)
</script>

<template>
  <div v-if="controls.length" class="filter-bar" role="search" aria-label="Filters">
    <div v-if="controls.includes('range')" class="range" role="group" aria-label="Date range">
      <button
        v-for="preset in RANGE_PRESETS"
        :key="preset"
        :class="{ active: filters.range === preset }"
        :aria-pressed="filters.range === preset"
        :title="RANGE_LABELS[preset]"
        @click="update({ range: preset })"
      >
        {{ preset }}
      </button>
    </div>
    <MultiSelect v-if="controls.includes('pipeline')" v-model="pipelines" label="Pipelines" all-label="All" :options="pipelineOptions" />
    <MultiSelect v-if="controls.includes('model')" v-model="models" label="Models" all-label="All" :options="modelOptions" />
    <input
      v-if="controls.includes('search')"
      v-model="search"
      class="search"
      type="search"
      placeholder="Search document or job ID"
      aria-label="Search jobs"
    />
    <button v-if="hasNarrowing" class="reset" @click="reset">Reset filters</button>
  </div>
</template>

<style scoped>
.filter-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-2);
  margin-bottom: var(--space-5);
}
.range {
  display: inline-flex;
  padding: 2px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
}
.range button {
  border: none;
  background: none;
  padding: 3px 12px;
  border-radius: 4px;
  color: var(--color-text-muted);
  font-weight: 550;
  cursor: pointer;
}
.range button.active {
  background: var(--color-accent-soft);
  color: var(--color-accent);
}
.search {
  min-width: 220px;
  flex: 0 1 280px;
  padding: 5px 10px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-text);
  font: inherit;
}
.search:focus-visible,
.range button:focus-visible {
  outline: 2px solid var(--color-accent);
  outline-offset: 1px;
}
.reset {
  border: none;
  background: none;
  color: var(--color-accent);
  cursor: pointer;
  font-weight: 550;
}
@media (max-width: 560px) {
  .search {
    flex-basis: 100%;
  }
}
</style>
