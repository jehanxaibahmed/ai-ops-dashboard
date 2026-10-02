<script setup lang="ts">
import { computed } from 'vue'
import { useJobsHub } from '@/shared/realtime/jobsHub'

const { status } = useJobsHub()
const label = computed(
  () =>
    ({
      connected: 'Live',
      connecting: 'Connecting…',
      reconnecting: 'Reconnecting…',
      disconnected: 'Offline',
    })[status.value],
)
</script>

<template>
  <span class="conn" :class="status" role="status" :title="`Live updates: ${status}`">
    <span class="dot" aria-hidden="true" />
    {{ label }}
  </span>
</template>

<style scoped>
.conn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--color-text-muted);
}
.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-neutral);
}
.connected .dot {
  background: var(--color-success);
  box-shadow: 0 0 0 3px var(--color-success-soft);
}
.connecting .dot,
.reconnecting .dot {
  background: var(--color-warning);
}
.disconnected .dot {
  background: var(--color-danger);
}
</style>
