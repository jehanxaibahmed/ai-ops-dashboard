<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { routes } from '@/app/router'
import FilterBar from '@/shared/components/filters/FilterBar.vue'

const route = useRoute()
const navItems = computed(() =>
  routes
    .filter((r) => r.meta?.title && !r.meta?.hidden)
    .map((r) => ({ name: r.name as string, title: r.meta!.title as string, path: r.path })),
)
</script>

<template>
  <div class="shell">
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-mark" aria-hidden="true">◆</span>
        <span>AI Ops</span>
      </div>
      <nav aria-label="Main">
        <RouterLink
          v-for="item in navItems"
          :key="item.name"
          :to="item.path"
          class="nav-link"
          :class="{ active: route.name === item.name }"
        >
          {{ item.title }}
        </RouterLink>
      </nav>
      <div class="sidebar-footer">
        <slot name="sidebar-footer" />
      </div>
    </aside>
    <main class="content">
      <FilterBar />
      <slot />
    </main>
  </div>
</template>

<style scoped>
.shell {
  display: grid;
  grid-template-columns: 220px minmax(0, 1fr);
  min-height: 100vh;
}
.sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  padding: var(--space-5) var(--space-4);
  border-right: 1px solid var(--color-border);
  background: var(--color-surface);
}
.brand {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-weight: 700;
  font-size: 1.05rem;
}
.brand-mark {
  color: var(--color-accent);
}
nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.nav-link {
  padding: var(--space-2) var(--space-3);
  border-radius: var(--radius-sm);
  color: var(--color-text-muted);
  text-decoration: none;
}
.nav-link:hover {
  background: var(--color-surface-hover);
  color: var(--color-text);
}
.nav-link.active {
  background: var(--color-accent-soft);
  color: var(--color-accent);
  font-weight: 600;
}
.sidebar-footer {
  margin-top: auto;
}
.content {
  padding: var(--space-6);
  min-width: 0;
}
@media (max-width: 760px) {
  .shell {
    grid-template-columns: minmax(0, 1fr);
  }
  .sidebar {
    position: static;
    height: auto;
    gap: var(--space-3);
    border-right: none;
    border-bottom: 1px solid var(--color-border);
    padding: var(--space-3) var(--space-4);
  }
  /* Nav scrolls sideways instead of pushing the page wider than the screen. */
  nav {
    flex-direction: row;
    overflow-x: auto;
    scrollbar-width: none;
    margin: 0 calc(-1 * var(--space-4));
    padding: 0 var(--space-4);
  }
  .nav-link {
    white-space: nowrap;
  }
  .sidebar-footer {
    margin-top: 0;
  }
  .content {
    padding: var(--space-4);
  }
}
</style>
