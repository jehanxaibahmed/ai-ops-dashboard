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

Each feature folder owns its API calls, Pinia store and components. Shared code lives in `shared/`. Features never import from each other, only from `shared/`.

## Live updates

The backend publishes job changes through `IJobNotifier`. The SignalR adapter pushes them to connected clients, and the frontend store merges each update into its local state.

## API surface

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/health` | Liveness check |
| GET | `/api/catalog` | Pipelines and models with pricing |
| GET | `/api/jobs` | Paged job list. Filters: `status`, `pipeline`, `model` (repeatable), `from`, `to`, `search` |
| GET | `/api/jobs/summary` | Counts by status, success rate, average duration, total cost |
| GET | `/api/jobs/{id}` | One job |
| WS | `/hubs/jobs` | SignalR. Server sends `JobUpdated(job)` on every change |

## Demo mode

`JobSimulationEngine` is a timer-free state machine driven by `JobSimulatorService`. On startup `HistorySeeder` writes 14 days of finished jobs so charts have data. Both are configured under `Simulation` in `appsettings.json`.
