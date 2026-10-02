<script setup lang="ts">
import AppShell from '@/layouts/AppShell.vue'
import ConnectionIndicator from '@/shared/components/ConnectionIndicator.vue'
import DemoControls from '@/shared/components/DemoControls.vue'
import ToastHost from '@/shared/components/ToastHost.vue'
</script>

<template>
  <AppShell>
    <template #sidebar-footer>
      <DemoControls />
      <ConnectionIndicator />
    </template>
    <RouterView />
  </AppShell>
  <ToastHost />
</template>
