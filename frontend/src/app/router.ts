import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'overview',
    component: () => import('@/features/overview/OverviewPage.vue'),
    meta: { title: 'Overview', filters: ['range', 'pipeline', 'model'] },
  },
  {
    path: '/jobs',
    name: 'jobs',
    component: () => import('@/features/jobs/JobsPage.vue'),
    meta: { title: 'Jobs', filters: ['range', 'pipeline', 'model', 'search'] },
  },
  {
    path: '/failures',
    name: 'failures',
    component: () => import('@/features/failures/FailuresPage.vue'),
    meta: { title: 'Failures', filters: ['range', 'pipeline', 'model', 'search'] },
  },
  {
    path: '/costs',
    name: 'costs',
    component: () => import('@/features/costs/CostsPage.vue'),
    meta: { title: 'Costs', filters: ['range', 'pipeline', 'model'] },
  },
  {
    path: '/accuracy',
    name: 'accuracy',
    component: () => import('@/features/accuracy/AccuracyPage.vue'),
    meta: { title: 'Accuracy', filters: ['range', 'pipeline', 'model'] },
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Keep the filter query string when moving between pages, so a filtered view stays filtered.
const FILTER_KEYS = ['range', 'pipeline', 'model', 'q']
router.beforeEach((to, from) => {
  if (to.path === from.path) return
  const carried = Object.fromEntries(Object.entries(from.query).filter(([k]) => FILTER_KEYS.includes(k)))
  const missing = Object.keys(carried).some((k) => !(k in to.query))
  if (missing && Object.keys(carried).length) return { ...to, query: { ...carried, ...to.query } }
})

router.afterEach((to) => {
  const title = to.meta.title as string | undefined
  document.title = title ? `${title} · AI Ops Dashboard` : 'AI Ops Dashboard'
})
