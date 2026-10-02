# Progress and handoff

Last updated: 2026-10-02 (end of session 2)

**Overall: about 85% done.**

## How the work is organised

The work is split into **stacked PRs**. Each branch is cut from the one before it, and each PR targets the previous branch. Review and merge them in order, 1 → 8. After you merge one, GitHub retargets the next PR to `main`; if it doesn't, change the base by hand.

| # | Branch | PR | State |
|---|---|---|---|
| 1 | `chore/01-project-scaffold` | [#1](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/1) | ✅ Ready for review |
| 2 | `feat/02-jobs-api` | [#2](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/2) | ✅ Ready for review |
| 3 | `feat/03-live-job-list` | [#3](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/3) | ✅ Ready for review |
| 4 | `feat/04-failures-retry` | [#4](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/4) | ✅ Ready for review |
| 5 | `feat/05-cost-analytics` | [#5](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/5) | ✅ Ready for review |
| 6 | `feat/06-accuracy-trends` | [#6](https://github.com/jehanxaibahmed/ai-ops-dashboard/pull/6) | ✅ Ready for review |
| 7 | `feat/07-filters-demo-mode` | not opened yet | 🟡 Code written and committed as WIP. Builds, tests pass. Not yet checked in the browser |
| 8 | `chore/08-docker-ci` | — | ⬜ Not started |

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

### 7 · Filters and demo mode (WIP, committed but not reviewed)
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

## Next session

### Finish PR 7
1. Run the backend and frontend, then check each page in the browser:
   - The filter bar shows the right controls on each page.
   - Changing a filter updates the URL and reloads the data.
   - Filters carry over when you navigate.
   - Reset clears the filters.
   - The multi-select closes when you click outside it or press Esc.
2. Check demo mode:
   - Pause stops new jobs.
   - 5× speed is visibly faster.
   - A 30% failure rate shows up on the Failures page.
   - A second tab picks up changes.
3. Check the sidebar layout at phone width, now that it holds the demo controls.
4. Optional: keep the Jobs status tab in the URL too (`?status=`).
5. Update `docs/ARCHITECTURE.md` with the filters, the `/api/simulation` endpoints and the `SimulationChanged` event.
6. Commit, then open PR 7 against `feat/06-accuracy-trends`.

### PR 8 · Docker, CI, README
- `backend/Dockerfile`: multi-stage build with the .NET 10 SDK, then the aspnet runtime. Listen on `8080`.
- `frontend/Dockerfile`: build with node, then serve with nginx. `nginx.conf` should:
  - fall back to `index.html` for SPA routes
  - proxy `/api` to the backend
  - proxy `/hubs` with the WebSocket upgrade headers
- `docker-compose.yml` with the backend and frontend services. Set CORS through environment variables.
- `.github/workflows/ci.yml` with two jobs:
  - backend: `dotnet test`
  - frontend: `npm ci`, `npm test`, `npm run build`
- Final README:
  - screenshots of each page
  - tick off every roadmap item
  - change the status badge
  - add a features section and a Docker quick start
- Open PR 8 against `feat/07-filters-demo-mode`.

### Nice to have, if time allows
- Code-split Chart.js so the Jobs and Overview pages don't load it.
- Retry a whole error code from the breakdown ("Retry all `rate_limited`").
- End-to-end smoke test with Playwright.

## Known limitations
- The simulator changes job objects in memory while the API may be reading them. That is acceptable for an in-memory demo; a database adapter would remove it.
- Retried jobs stay `Queued` while demo mode is paused. That is expected.
- The `24h` range is computed when the filters change. It does not roll forward while a page stays open.

## Running locally
```bash
cd backend && dotnet run --project src/AiOps.Api          # http://localhost:5080
cd frontend && npm install && npm run dev                 # http://localhost:5173
```
