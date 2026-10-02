# Architecture

AI Ops Dashboard is a monorepo with a .NET backend and a Vue frontend.

```
ai-ops-dashboard/
├── backend/                     ASP.NET Core (.NET 10)
│   ├── src/
│   │   ├── AiOps.Domain/        Entities, value objects, domain rules. No dependencies.
│   │   ├── AiOps.Application/   Use cases, ports (interfaces), DTOs, queries.
│   │   ├── AiOps.Infrastructure/ Adapters: in-memory store, job simulator, clock.
│   │   └── AiOps.Api/           HTTP endpoints, SignalR hub, composition root.
│   └── tests/
│       ├── AiOps.UnitTests/         Domain, application and simulator tests
│       └── AiOps.Api.IntegrationTests/ HTTP tests via WebApplicationFactory
├── frontend/                    Vue 3 + TypeScript + Vite
│   └── src/
│       ├── app/                 App root and router
│       ├── layouts/             Page shell
│       ├── features/<name>/     One folder per feature: api, store, components, page
│       ├── shared/              Cross-feature api client, components, utilities
│       └── styles/              Design tokens and global CSS
└── docs/
```

## Backend layers

Dependencies point inwards: `Api → Infrastructure → Application → Domain`.

- **Domain** holds the job lifecycle rules (status transitions, retries). It knows nothing about storage or HTTP.
- **Application** defines ports such as `IJobRepository` and `IJobNotifier`, and the services that the API calls.
- **Infrastructure** implements the ports. Storage is in-memory because the project uses synthetic data only; swapping it for a database means adding one adapter.
- **Api** wires everything together, exposes REST endpoints under `/api` and a SignalR hub under `/hubs/jobs`.

## Frontend structure

- `shared/api/` holds the typed endpoint clients and the DTO types that mirror the backend.
- `shared/realtime/` owns the single SignalR connection. Stores subscribe with `onJobUpdated(listener, onReconnected)`.
- `shared/components/`, `shared/utils/`, `shared/stores/` hold reusable UI, formatters and the catalog store.
- Each `features/<name>/` folder owns its page, Pinia store and components.

Features never import from each other, only from `shared/`. That keeps every feature removable on its own.

## Live updates

The backend publishes job changes through `IJobNotifier`. The SignalR adapter pushes them to connected clients, and the frontend store merges each update into its local state.

## API surface

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/health` | Liveness check |
| GET | `/api/catalog` | Pipelines and models with pricing |
| GET | `/api/jobs` | Paged job list. Filters: `status`, `pipeline`, `model`, `failureCode` (repeatable), `from`, `to`, `search` |
| GET | `/api/jobs/summary` | Counts by status, success rate, average duration, total cost |
| GET | `/api/jobs/{id}` | One job |
| POST | `/api/jobs/{id}/retry` | Retry one failed job. `409` if it is not failed or has no attempts left |
| POST | `/api/jobs/retry` | Bulk retry `{ jobIds: [] }`. Returns `retried` and `skipped` with reasons |
| GET | `/api/failures/breakdown` | Failures grouped by error code and failure rate per pipeline |
| GET | `/api/analytics/costs` | Spend totals, by model, by pipeline and per UTC day by model |
| GET | `/api/analytics/accuracy` | Field-weighted accuracy overall, per pipeline or model (`groupBy`), per day, and the most-missed fields |
| GET | `/api/simulation` | Demo-mode state: running, speed, failure rate |
| PATCH | `/api/simulation` | Update any of `running`, `speed` (0.5/1/2/5), `failureRate` (0–0.9). `400` on invalid values |
| WS | `/hubs/jobs` | SignalR. Server sends `JobUpdated(job)` on every job change and `SimulationChanged(state)` on demo-mode changes |

## Demo mode

`JobSimulationEngine` is a timer-free state machine driven by `JobSimulatorService`. On startup `HistorySeeder` writes 14 days of finished jobs so charts have data. Both are configured under `Simulation` in `appsettings.json`.

At runtime, `ISimulationControl` holds the live settings. Pausing skips ticks. Changing speed changes the tick period of the `PeriodicTimer`. The engine reads the failure rate on every tick. The sidebar controls call `PATCH /api/simulation`, and every open tab updates through `SimulationChanged`.

## Retries

`Job.Retry` is only allowed from `Failed` and at most `Job.MaxAttempts` (5) times in total. A retry puts the job back to `Queued` as a new attempt. Token usage and cost add up across attempts, because failed attempts still cost money.

## Charts

Charts use Chart.js through `vue-chartjs`, wrapped in `shared/components/charts/`.

- Series colours are CSS tokens (`--series-1..4`) with separate light and dark steps. Both sets pass the colour-blind separation checks.
- A model's colour comes from its position in the catalog, not its rank, so filtering never repaints the remaining series.
- Every chart sits in a `ChartCard` with a **Table** view. The table is the accessible fallback, and it is needed because two light-mode colours are below 3:1 contrast.
- `useChartTheme` re-reads the tokens when the OS colour scheme changes.

## Evaluations

Each succeeded job gets an `EvaluationResult`: the pipeline's fields that were checked and the ones the model got wrong. Accuracy is field-weighted (correct fields / checked fields).

`SampleEvaluator` gives each field a chance of being right that depends on the model, the pipeline and the field. It also builds in a regression: Gemini Flash is 12 points worse over the last 4 days. That gives the demo a visible drop to investigate on the Accuracy page.

## Filters

Pages share one set of filters, kept in the URL so any view can be linked:

```
?range=24h|7d|14d   (default 7d, omitted from the URL)
&pipeline=a&pipeline=b
&model=m
&q=search
&status=Failed      (Jobs page only)
```

- `shared/filters/filters.ts` parses and serialises the query string and turns it into the API `JobQuery`.
- `useGlobalFilters()` reads the filters from the route and exposes a stable `key` that pages watch to reload.
- Each route lists the controls it supports in `meta.filters`, for example the Costs page has no search box.
- `FilterBar` renders the controls in a single row above the page.
- The router carries the filter keys over when you move between pages.
