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
│       └── AiOps.UnitTests/
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
