# Progress and handoff

Last updated: 2026-10-02

## How the work is organised

The work is split into **stacked PRs**. Each branch is cut from the one before it, and each PR targets the previous branch. Review and merge them in order, 1 → 8. After you merge one, GitHub retargets the next PR to `main`, or you can change its base by hand.

| # | Branch | PR | State |
|---|---|---|---|
| 1 | `chore/01-project-scaffold` | [#1](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/1) | ✅ Open, ready for review |
| 2 | `feat/02-jobs-api` | [#2](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/2) | ✅ Open, ready for review |
| 3 | `feat/03-live-job-list` | [#3](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/3) | ✅ Open, ready for review |
| 4 | `feat/04-failures-retry` | [#4](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/4) | ✅ Open, ready for review |
| 5 | `feat/05-cost-analytics` | not opened yet | 🟡 Backend done, frontend not started |
| 6 | `feat/06-accuracy-trends` | — | ⬜ Not started |
| 7 | `feat/07-filters-demo-mode` | — | ⬜ Not started |
| 8 | `chore/08-docker-ci` | — | ⬜ Not started |

## Done

### 1 · Scaffold
- .NET 10 solution `backend/AiOps.slnx` with four layers: `Domain → Application → Infrastructure → Api`. A shared `Directory.Build.props` turns on nullable and treats warnings as errors.
- Vue 3 + TS + Vite frontend with Vue Router, Pinia and `@microsoft/signalr`. The folders are `app/`, `layouts/`, `features/<name>/`, `shared/` and `styles/`.
- Design tokens with light and dark mode. Typed `request` and `buildQuery` helpers. The Vite dev proxy forwards `/api` and `/hubs` to `:5080`.
- `docs/ARCHITECTURE.md` describes the layer rules and the API surface.

### 2 · Job domain, API, realtime, simulator
- `Job` entity with an enforced lifecycle (`Queued → Running → Succeeded | Failed`), token usage and cost.
- `JobService`: paging, filtering with `JobFilter`, and summary stats.
- `InMemoryJobRepository`, bounded at 5,000 jobs.
- `SampleCatalog` with 4 pipelines and 4 models. The pricing is illustrative.
- `JobSimulationEngine` is timer-free and seedable. It is driven by `JobSimulatorService`.
- `HistorySeeder` writes 14 days × 90 jobs.
- Endpoints: `/api/health`, `/api/catalog`, `/api/jobs`, `/api/jobs/summary`, `/api/jobs/{id}`.
- SignalR hub at `/hubs/jobs` sends `JobUpdated` events. Errors are returned as ProblemDetails.

### 3 · Live job list and overview
- One shared SignalR connection with reconnect handling and a Live / Offline indicator.
- Jobs page: status tabs, paged table, detail side panel, and a pause/resume control for live updates.
- Overview page: 24h KPIs and a list of running jobs.
- Shared UI kit: `StatusBadge`, `ProgressBar`, `StatCard`, `Card`, `SidePanel`, `PaginationBar`, `AppButton`, `EmptyState`.

### 4 · Failures and retry
- `Job.Retry` allows at most 5 attempts. Cost adds up across attempts.
- Single retry endpoint (returns `409` if the job can't be retried) and bulk retry endpoint (returns skip reasons).
- `/api/failures/breakdown` groups failures by error code and gives the failure rate per pipeline.
- Failures page: error-code bars you can click to filter, pipeline failure rates, a selectable table, **Retry selected** and **Retry transient**. The detail panel also has a retry button.
- Toast notifications.

### 5 · Cost analytics (partial, committed on `feat/05-cost-analytics`)
- `CostAnalyticsService` and `GET /api/analytics/costs`, which accepts the same filters as `/api/jobs`.
- The response has totals (including the cost of failed jobs), breakdowns by model and by pipeline, and a daily series by model. Empty days are filled with zero.
- Models are listed in catalog order, so each model keeps the same chart colour.
- 4 unit tests.

### Tests at this point
- Backend: 32 unit tests and 8 integration tests, all passing.
- Frontend: 15 tests, all passing. The build is clean.

## Left for next session

### 5 · Cost analytics: frontend
- `npm i chart.js vue-chartjs`.
- Add `getCostReport` to `shared/api` and add the DTO types.
- Cost page at `/costs`:
  - Stat tiles: total, average per job, tokens, failed spend.
  - **Stacked columns of daily cost by model.**
  - Bar chart of cost by model.
  - Table by pipeline.
- Chart rules, from the dataviz skill:
  - Model colours, light mode: `#2a78d6, #eb6834, #1baf7a, #eda100`.
  - Model colours, dark mode: `#3987e5, #d95926, #199e70, #c98500`.
  - Both sets were checked with the palette validator and pass. In light mode, aqua and yellow fall below 3:1 contrast, so **each chart needs a table-view toggle**.
  - Assign colours by model in catalog order, not by rank.
  - Always show a legend, add tooltips, use 4px rounded bar ends and a 2px surface gap.
  - Read colours from CSS variables, and re-render when the colour scheme changes.
- Integration test for `/api/analytics/costs`.
- Open PR 5 against `feat/04-failures-retry`.

### 6 · Accuracy trends
- Domain: `EvaluationResult` with job, pipeline, model, fields total and fields correct, accuracy, and evaluation time.
- The simulator and seeder write one for each succeeded job. Accuracy should vary by model and pipeline.
- `GET /api/analytics/accuracy`: daily accuracy per pipeline or model, plus an overall figure and the worst fields.
- Frontend: line chart per pipeline (4 series, 2px lines, crosshair tooltip, table view) and accuracy KPIs.

### 7 · Filters and demo mode
- Filter bar shared by every page, in one row above the content:
  - Date range presets (24h / 7d / 14d / custom).
  - Pipeline and model multi-selects.
  - Search.
- Keep the filter in the URL query string. Every page's store reads that filter.
- Demo mode: an `ISimulationControl` service and `GET/POST /api/simulation` to pause, resume and change speed and failure rate. Add a toggle in the sidebar.
- Tick off the README roadmap items.

### 8 · Docker and CI
- Backend Dockerfile, using a multi-stage build.
- Frontend Dockerfile: build the app, serve it with nginx, and proxy `/api` and `/hubs` with WebSocket upgrade.
- `docker-compose.yml`.
- GitHub Actions workflow that runs `dotnet test`, `npm ci`, `npm test` and `npm run build`.
- Final README with screenshots and a status badge.

## Known limitations and notes
- The simulator changes job objects in memory while the API may be reading them. That is acceptable for an in-memory demo. A database adapter would remove the problem.
- On the Failures page, the error-code filter works on the loaded page only, because the API has no `code` filter yet. That filter could be added in PR 7.
- Retried jobs stay `Queued` if the simulator is disabled.

## Running locally
```bash
cd backend && dotnet run --project src/AiOps.Api          # http://localhost:5080
cd frontend && npm install && npm run dev                 # http://localhost:5173
```
