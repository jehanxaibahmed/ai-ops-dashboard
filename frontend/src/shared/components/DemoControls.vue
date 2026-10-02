<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { FAILURE_RATES, SPEEDS, useSimulationStore } from '@/shared/stores/simulation'

const store = useSimulationStore()
const { state, saving } = storeToRefs(store)
onMounted(() => void store.init())
</script>

<template>
  <section v-if="state" class="demo" aria-label="Demo mode">
    <div class="head">
      <span class="title">Demo mode</span>
      <button
        class="switch"
        role="switch"
        :aria-checked="state.running"
        :disabled="saving"
        :title="state.running ? 'Pause simulated jobs' : 'Resume simulated jobs'"
        @click="store.update({ running: !state.running })"
      >
        <span class="knob" />
      </button>
    </div>
    <p class="status">{{ state.running ? 'Simulating live jobs' : 'Paused' }}</p>

    <div class="row" role="group" aria-label="Speed">
      <span class="label">Speed</span>
      <button
        v-for="s in SPEEDS"
        :key="s"
        :class="{ active: state.speed === s }"
        :aria-pressed="state.speed === s"
        :disabled="saving"
        @click="store.update({ speed: s })"
      >
        {{ s }}×
      </button>
    </div>
    <div class="row" role="group" aria-label="Failure rate">
      <span class="label">Failures</span>
      <button
        v-for="r in FAILURE_RATES"
        :key="r"
        :class="{ active: Math.abs(state.failureRate - r) < 1e-9 }"
        :aria-pressed="Math.abs(state.failureRate - r) < 1e-9"
        :disabled="saving"
        @click="store.update({ failureRate: r })"
      >
        {{ Math.round(r * 100) }}%
      </button>
    </div>
  </section>
</template>

<style scoped>
.demo {
  display: grid;
  gap: var(--space-2);
  padding: var(--space-3);
  margin-bottom: var(--space-3);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: 12px;
}
.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.title {
  font-weight: 650;
  font-size: 13px;
}
.status {
  margin: -4px 0 0;
  color: var(--color-text-muted);
}
.switch {
  position: relative;
  width: 34px;
  height: 20px;
  border: none;
  border-radius: 999px;
  background: var(--color-neutral-soft);
  cursor: pointer;
  padding: 0;
}
.switch[aria-checked='true'] {
  background: var(--color-accent);
}
.knob {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: #fff;
  transition: transform 0.15s;
}
.switch[aria-checked='true'] .knob {
  transform: translateX(14px);
}
.row {
  display: flex;
  align-items: center;
  gap: 2px;
}
.label {
  width: 52px;
  color: var(--color-text-muted);
}
.row button {
  flex: 1;
  padding: 2px 0;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: var(--color-surface);
  color: var(--color-text-muted);
  cursor: pointer;
  font-size: 11px;
}
.row button.active {
  border-color: var(--color-accent);
  background: var(--color-accent-soft);
  color: var(--color-accent);
  font-weight: 600;
}
.switch:focus-visible,
.row button:focus-visible {
  outline: 2px solid var(--color-accent);
  outline-offset: 1px;
}
</style>
