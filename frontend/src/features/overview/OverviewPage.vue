<script setup lang="ts">
import { onMounted, ref } from 'vue'
import PageHeader from '@/shared/components/PageHeader.vue'
import { request } from '@/shared/api/http'

const apiStatus = ref<'checking' | 'ok' | 'down'>('checking')

onMounted(async () => {
  try {
    await request<{ status: string }>('/api/health')
    apiStatus.value = 'ok'
  } catch {
    apiStatus.value = 'down'
  }
})
</script>

<template>
  <PageHeader title="Overview" subtitle="Live view of AI processing pipelines" />
  <p>API status: <strong>{{ apiStatus }}</strong></p>
</template>
