<script setup lang="ts">
import { computed } from 'vue'
import type { FailureCodeStat } from '@/shared/api/types'
import { formatNumber } from '@/shared/utils/format'

const props = defineProps<{ codes: FailureCodeStat[]; active: string | null }>()
const emit = defineEmits<{ select: [code: string] }>()
const max = computed(() => Math.max(1, ...props.codes.map((c) => c.count)))
</script>

<template>
  <ul class="codes">
    <li v-for="c in codes" :key="c.code">
      <button :class="{ active: active === c.code }" :aria-pressed="active === c.code" @click="emit('select', c.code)">
        <div class="row">
          <span class="code">{{ c.code }}</span>
          <span class="tag" :class="c.isTransient ? 'transient' : 'permanent'">{{ c.isTransient ? 'transient' : 'permanent' }}</span>
          <span class="count tabular">{{ formatNumber(c.count) }}</span>
        </div>
        <div class="bar"><div class="fill" :style="{ width: `${(c.count / max) * 100}%` }" /></div>
        <div class="msg">{{ c.sampleMessage }}</div>
      </button>
    </li>
  </ul>
</template>

<style scoped>
.codes {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 2px;
}
button {
  width: 100%;
  text-align: left;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  background: none;
  color: inherit;
  padding: var(--space-2);
  cursor: pointer;
}
button:hover {
  background: var(--color-surface-hover);
}
button.active {
  border-color: var(--color-accent);
  background: var(--color-accent-soft);
}
.row {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}
.code {
  font-family: var(--font-mono);
  font-weight: 600;
}
.count {
  margin-left: auto;
  font-weight: 600;
}
.tag {
  font-size: 11px;
  padding: 0 6px;
  border-radius: 999px;
}
.transient {
  color: var(--color-warning);
  background: var(--color-warning-soft);
}
.permanent {
  color: var(--color-danger);
  background: var(--color-danger-soft);
}
.bar {
  margin: 6px 0 4px;
  height: 6px;
  border-radius: 999px;
  background: var(--color-neutral-soft);
}
.fill {
  height: 100%;
  border-radius: inherit;
  background: var(--color-danger);
}
.msg {
  color: var(--color-text-muted);
  font-size: 12px;
}
</style>
