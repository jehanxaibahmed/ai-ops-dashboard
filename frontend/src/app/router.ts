import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'overview',
    component: () => import('@/features/overview/OverviewPage.vue'),
    meta: { title: 'Overview' },
  },
  {
    path: '/jobs',
    name: 'jobs',
    component: () => import('@/features/jobs/JobsPage.vue'),
    meta: { title: 'Jobs' },
  },
  {
    path: '/failures',
    name: 'failures',
    component: () => import('@/features/failures/FailuresPage.vue'),
    meta: { title: 'Failures' },
  },
  {
    path: '/costs',
    name: 'costs',
    component: () => import('@/features/costs/CostsPage.vue'),
    meta: { title: 'Costs' },
  },
  {
    path: '/accuracy',
    name: 'accuracy',
    component: () => import('@/features/accuracy/AccuracyPage.vue'),
    meta: { title: 'Accuracy' },
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.afterEach((to) => {
  const title = to.meta.title as string | undefined
  document.title = title ? `${title} · AI Ops Dashboard` : 'AI Ops Dashboard'
})
