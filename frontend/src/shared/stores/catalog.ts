import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { getCatalog } from '@/shared/api/catalog'
import type { Model, Pipeline } from '@/shared/api/types'

/** Pipelines and models. Loaded once and used for labels and filter options. */
export const useCatalogStore = defineStore('catalog', () => {
  const pipelines = ref<Pipeline[]>([])
  const models = ref<Model[]>([])
  let loading: Promise<void> | null = null

  const pipelineById = computed(() => new Map(pipelines.value.map((p) => [p.id, p])))
  const modelById = computed(() => new Map(models.value.map((m) => [m.id, m])))

  function ensureLoaded() {
    loading ??= getCatalog()
      .then((c) => {
        pipelines.value = c.pipelines
        models.value = c.models
      })
      .catch((e) => {
        loading = null
        throw e
      })
    return loading
  }

  const pipelineName = (id: string) => pipelineById.value.get(id)?.name ?? id
  const modelName = (id: string) => modelById.value.get(id)?.displayName ?? id

  return { pipelines, models, ensureLoaded, pipelineName, modelName }
})
