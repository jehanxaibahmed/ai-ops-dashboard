# Progress and handoff

Last updated: 2026-10-02 (end of session 3)

**Overall: 100% of the planned roadmap is done.** All 8 PRs are open and ready for review.

## How the work is organised

The work is split into **stacked PRs**. Each branch is cut from the one before it, and each PR targets the previous branch. Review and merge them in order, 1 → 8. After you merge one, GitHub retargets the next PR to `main`; if it doesn't, change the base by hand.

| # | Branch | PR | What |
|---|---|---|---|
| 1 | `chore/01-project-scaffold` | [#1](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/1) | Layered .NET 10 backend and Vue 3 frontend scaffold |
| 2 | `feat/02-jobs-api` | [#2](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/2) | Job domain, REST API, SignalR hub, simulator |
| 3 | `feat/03-live-job-list` | [#3](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/3) | Live job list and overview |
| 4 | `feat/04-failures-retry` | [#4](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/4) | Failure view with retry actions |
| 5 | `feat/05-cost-analytics` | [#5](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/5) | Cost per model and per day |
| 6 | `feat/06-accuracy-trends` | [#6](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/6) | Accuracy trends from evaluation results |
| 7 | `feat/07-filters-demo-mode` | [#7](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/7) | Global URL filters and demo-mode controls |
| 8 | `chore/08-docker-ci` | [#8](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/8) | Docker Compose, GitHub Actions CI, final README |

## Done

### 1–4 (from session 1)
- **Scaffold.** Layered .NET 10 backend and a Vue 3 + TS frontend organised by feature.
- **Backend core.** Job lifecycle, REST API, SignalR hub, simulator, and 14 days of seeded history.
- **Live pages.** Jobs page and Overview page with live updates.
- **Failures.** Failures page with single and bulk retry, and a breakdown by error code and pipeline.

### 5 · Costs
- `GET /api/analytics/costs`.
- Costs page with stat tiles, stacked daily cost by model, a cost-by-model bar chart, and a pipeline table.
- Shared chart kit (`shared/components/charts/`):
  - Chart.js setup
  - a theme read from CSS tokens that follows dark mode
  - a `ChartCard` that switches between chart and table view
  - a legend component
- The palette passed the colour-blind and normal-vision checks in both light and dark mode.

### 6 · Accuracy
- `EvaluationResult` entity, plus a synthetic evaluator. It builds in a Gemini Flash regression of −12 pts over the last 4 days, so the demo has a drop to find.
- `GET /api/analytics/accuracy?groupBy=Pipeline|Model`.
- Accuracy page:
  - trend line chart with a crosshair tooltip
  - table that flags drops of 2 points or more
  - list of the most-missed fields

### 7 · Filters and demo mode
- **Backend**
  - `failureCode` filter on all job endpoints. This fixes the old limitation where the error-code filter only covered the loaded page.
  - `ISimulationControl` with running, speed (0.5/1/2/5×) and failure rate.
  - `GET` and `PATCH /api/simulation`. A `PATCH` is broadcast over SignalR as `SimulationChanged`.
  - The simulator loop now honours pause and speed at runtime.
  - Tests added. Backend total: 48 unit + 14 integration, all passing.
- **Frontend**
  - The filter model (`shared/filters/`) is kept in the URL: `?range=24h|7d|14d&pipeline=…&model=…&q=…`. It is tested.
  - `FilterBar` sits in one row above every page. Each route's `meta.filters` decides which controls it shows.
  - Filters carry over when you move between pages.
  - `MultiSelect` dropdown component.
  - Every page now reads the global filters: Overview, Jobs, Failures, Costs and Accuracy. The local 7/14-day buttons on Costs and Accuracy were removed.
  - The Failures error-code filter now runs on the server.
  - The sidebar has demo-mode controls (on/off switch, speed, failure rate) that stay in sync across tabs.
  - Frontend total: 22 tests passing, clean build.

### 7 · Follow-up fixes (session 3)
- The sidebar is sticky on desktop.
- Mobile layout fixed: at 390 px the page no longer scrolls sideways.
- The Jobs status tab is kept in the URL.

### 8 · Docker, CI, README
- `backend/Dockerfile`: multi-stage build with the .NET 10 SDK, then the aspnet runtime. Runs as a non-root user on port 8080.
- `frontend/Dockerfile`: node build, then nginx. `nginx.conf` falls back to `index.html` for SPA routes, proxies `/api`, proxies `/hubs` with WebSocket upgrade, and caches the hashed assets.
- `docker-compose.yml`: web on `:8080`, api on `:5080`.
- `.github/workflows/ci.yml`: a backend job (build + tests), a frontend job (tests + build) and a Docker build job.
- README rewritten with features, screenshots, quick start, architecture summary and the roadmap ticked off.

### Tests
- Backend: 48 unit + 14 integration tests.
- Frontend: 22 tests, clean type-checked build.
- Docker: ran the compose stack, then checked the health endpoint, an SPA deep link, and SignalR over WebSocket through nginx (18 events in 4 s).

## If you continue later (optional)
- Database-backed store, such as Postgres with EF Core. It would replace the in-memory repositories behind the existing ports.
- An ingestion API for real evaluation results.
- Authentication, and per-team views.
- Alerts when accuracy drops or the failure rate spikes.
- Code-split Chart.js so the Jobs and Overview pages don't load it.
- End-to-end smoke tests with Playwright.

## Known limitations
- The simulator changes job objects in memory while the API may be reading them. That is acceptable for an in-memory demo; a database adapter would remove it.
- Retried jobs stay `Queued` while demo mode is paused. That is expected.
- The `24h` range is computed when the filters change. It does not roll forward while a page stays open.

## Running locally
```bash
docker compose up --build                                 # http://localhost:8080
# or
cd backend && dotnet run --project src/AiOps.Api          # http://localhost:5080
cd frontend && npm install && npm run dev                 # http://localhost:5173
```
