<script setup lang="ts">
import AppShell from '@/layouts/AppShell.vue'
import ConnectionIndicator from '@/shared/components/ConnectionIndicator.vue'
</script>

<template>
  <AppShell>
    <template #sidebar-footer>
      <ConnectionIndicator />
    </template>
    <RouterView />
  </AppShell>
</template>
