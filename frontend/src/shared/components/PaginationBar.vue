<script setup lang="ts">
import { computed } from 'vue'
import AppButton from './AppButton.vue'
import { formatNumber } from '@/shared/utils/format'

const props = defineProps<{ page: number; pageSize: number; total: number }>()
const emit = defineEmits<{ 'update:page': [page: number] }>()

const pages = computed(() => Math.max(1, Math.ceil(props.total / props.pageSize)))
const first = computed(() => (props.total === 0 ? 0 : (props.page - 1) * props.pageSize + 1))
const last = computed(() => Math.min(props.total, props.page * props.pageSize))
</script>

<template>
  <div class="pager">
    <span class="range tabular">{{ formatNumber(first) }}–{{ formatNumber(last) }} of {{ formatNumber(total) }}</span>
    <div class="buttons">
      <AppButton size="sm" :disabled="page <= 1" @click="emit('update:page', page - 1)">Previous</AppButton>
      <span class="tabular">Page {{ page }} / {{ pages }}</span>
      <AppButton size="sm" :disabled="page >= pages" @click="emit('update:page', page + 1)">Next</AppButton>
    </div>
  </div>
</template>

<style scoped>
.pager {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-2);
  padding-top: var(--space-3);
  color: var(--color-text-muted);
  font-size: 12px;
}
.buttons {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}
</style>
