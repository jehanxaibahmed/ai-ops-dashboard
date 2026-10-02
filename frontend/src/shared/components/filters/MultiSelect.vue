<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, useId } from 'vue'

const props = defineProps<{ label: string; allLabel: string; options: { value: string; label: string }[] }>()
const model = defineModel<string[]>({ required: true })

const open = ref(false)
const root = ref<HTMLElement | null>(null)
const id = useId()

const summary = computed(() => {
  if (model.value.length === 0) return props.allLabel
  if (model.value.length === 1) return props.options.find((o) => o.value === model.value[0])?.label ?? model.value[0]
  return `${model.value.length} ${props.label.toLowerCase()}`
})

function toggle(value: string) {
  model.value = model.value.includes(value) ? model.value.filter((v) => v !== value) : [...model.value, value]
}

function onDocClick(e: MouseEvent) {
  if (open.value && root.value && !root.value.contains(e.target as Node)) open.value = false
}
function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') open.value = false
}
onMounted(() => {
  document.addEventListener('click', onDocClick)
  document.addEventListener('keydown', onKey)
})
onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
  document.removeEventListener('keydown', onKey)
})
</script>

<template>
  <div ref="root" class="multi">
    <button
      class="trigger"
      :class="{ active: model.length > 0 }"
      :aria-expanded="open"
      :aria-controls="id"
      aria-haspopup="listbox"
      @click="open = !open"
    >
      <span class="label">{{ label }}:</span> {{ summary }}
      <span class="caret" aria-hidden="true">▾</span>
    </button>
    <div v-if="open" :id="id" class="menu" role="listbox" :aria-label="label" aria-multiselectable="true">
      <label v-for="o in options" :key="o.value" class="option">
        <input type="checkbox" :checked="model.includes(o.value)" @change="toggle(o.value)" />
        {{ o.label }}
      </label>
      <button v-if="model.length" class="clear" @click="model = []">Clear</button>
    </div>
  </div>
</template>

<style scoped>
.multi {
  position: relative;
}
.trigger {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 5px 10px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: 13px;
  cursor: pointer;
  white-space: nowrap;
}
.trigger:hover {
  background: var(--color-surface-hover);
}
.trigger.active {
  border-color: var(--color-accent);
}
.label {
  color: var(--color-text-muted);
}
.caret {
  color: var(--color-text-muted);
  font-size: 10px;
}
.menu {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  z-index: 30;
  min-width: 200px;
  padding: var(--space-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  box-shadow: 0 8px 24px rgb(0 0 0 / 18%);
}
.option {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: 6px var(--space-2);
  border-radius: 4px;
  cursor: pointer;
}
.option:hover {
  background: var(--color-surface-hover);
}
.clear {
  width: 100%;
  margin-top: var(--space-1);
  padding: 6px;
  border: none;
  border-top: 1px solid var(--color-border);
  background: none;
  color: var(--color-accent);
  cursor: pointer;
}
</style>
