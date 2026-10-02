# 📊 AI Ops Dashboard

![Status](https://img.shields.io/badge/status-in%20progress-orange?style=for-the-badge) ![Vue.js](https://img.shields.io/badge/Vue.js-4FC08D?style=for-the-badge&logo=vuedotjs&logoColor=white) ![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)

> A live dashboard for monitoring AI processing pipelines: job status, model cost and extraction accuracy in real time.

## 🎯 Why this project

When AI pipelines run in production, operations teams need to see what's happening: which jobs are running, which failed, how much each model costs and how accurate the results are. This dashboard streams live updates from a .NET backend with SignalR.

## 🧱 Stack

- Vue 3, TypeScript, Vite, Pinia and Vue Router
- SignalR for live updates
- ASP.NET Core (.NET 10) backend with background jobs
- Chart library for cost and accuracy trends
- Docker Compose for local setup

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the folder layout and layer rules.

## 🚀 Running locally

```bash
# Backend (http://localhost:5080)
cd backend
dotnet run --project src/AiOps.Api

# Frontend (http://localhost:5173, proxies /api and /hubs to the backend)
cd frontend
npm install
npm run dev
```

## 🗺️ Roadmap

- [ ] Live job list with status and progress
- [ ] Failure view with retry actions
- [ ] Cost per model and per day
- [ ] Accuracy trends from evaluation results
- [ ] Filters by pipeline and date
- [ ] Demo mode with simulated jobs

## 📌 Status

🚧 This project is in early development. It uses synthetic sample data only.

---

Built by [Jahanzaib Ahmad](https://github.com/jehanxaibahmed) · Full Stack Engineer · AI & LLM Systems
