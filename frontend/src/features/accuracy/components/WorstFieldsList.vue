<script setup lang="ts">
import { computed } from 'vue'
import type { FieldErrorStat } from '@/shared/api/types'
import EmptyState from '@/shared/components/EmptyState.vue'
import { useCatalogStore } from '@/shared/stores/catalog'
import { formatNumber, formatPercent } from '@/shared/utils/format'

const props = defineProps<{ fields: FieldErrorStat[] }>()
const catalog = useCatalogStore()
const max = computed(() => Math.max(0.0001, ...props.fields.map((f) => f.errorRate)))
</script>

<template>
  <EmptyState v-if="fields.length === 0" title="No field errors" />
  <ul v-else class="fields">
    <li v-for="f in fields" :key="`${f.pipelineId}:${f.field}`" :title="`${f.misses} of ${f.checked} wrong`">
      <div class="row">
        <span class="field">{{ f.field }}</span>
        <span class="pipeline">{{ catalog.pipelineName(f.pipelineId) }}</span>
        <span class="rate tabular">{{ formatPercent(f.errorRate) }}</span>
      </div>
      <div class="bar"><div class="fill" :style="{ width: `${(f.errorRate / max) * 100}%` }" /></div>
      <div class="meta">{{ formatNumber(f.misses) }} misses in {{ formatNumber(f.checked) }} documents</div>
    </li>
  </ul>
</template>

<style scoped>
.fields {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: var(--space-3);
}
.row {
  display: flex;
  align-items: baseline;
  gap: var(--space-2);
}
.field {
  font-family: var(--font-mono);
  font-weight: 600;
}
.pipeline,
.meta {
  color: var(--color-text-muted);
  font-size: 12px;
}
.rate {
  margin-left: auto;
  font-weight: 600;
}
.bar {
  margin: 4px 0 2px;
  height: 6px;
  border-radius: 999px;
  background: var(--color-neutral-soft);
}
.fill {
  height: 100%;
  border-radius: inherit;
  background: var(--color-warning);
}
</style>
