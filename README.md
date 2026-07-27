# Nimbus Board

Lightweight Jira alternative built on **Umbraco CMS**, **EF Core**, **MediatR**, **HTMX**, **SignalR**, and **Chart.js**.

## Solution layout

```text
NimbusBoard.slnx
docs/                          Architecture and API flow docs
src/
  Domain/NimbusBoard.Domain            Entities + enums
  Core/NimbusBoard.Application         MediatR use cases + interfaces
  Infrastructure/NimbusBoard.Infrastructure   EF Core, SMTP, factories
  Host/NimbusBoard.Web                 Umbraco host + Razor Pages UI
  Packaging/                           Publish / deploy notes
tests/NimbusBoard.Application.Tests
```

This mirrors a Clean Architecture layout (Domain / Core / Infrastructure / Host), similar to professional .NET solutions. Presentation (Razor Pages, hubs, composers) lives in **Host** because Umbraco is the web entry point.

## Features

- Dashboard with KPI cards, urgent tasks, sprint preview, and burndown chart
- Projects, issues (NIM-### keys), and Kanban boards with drag-and-drop
- Comments, labels, attachments, and activity log
- Sprint planning (create / start / complete / assign) with live burndown snapshots
- Assign issues to project members (Jira-style assignee picker)
- In-app notifications with SignalR badge updates and optional SMTP email
- Global search via **⌘K** / **Ctrl+K**

## Stack

| Layer | Project | Tech |
|---|---|---|
| Domain | `NimbusBoard.Domain` | Entities, enums |
| Core | `NimbusBoard.Application` | MediatR commands/queries |
| Infrastructure | `NimbusBoard.Infrastructure` | EF Core SQLite, SMTP |
| Host | `NimbusBoard.Web` | Umbraco 17 + Razor Pages `/app/*` |
| UI | (inside Host) | Tailwind, HTMX, Chart.js, Sortable.js, SignalR |

## Run locally

```bash
dotnet restore NimbusBoard.slnx
dotnet run --project src/Host/NimbusBoard.Web/NimbusBoard.Web.csproj
```

Open the app URL (typically `https://localhost:44386/app/dashboard`).

Umbraco unattended install credentials live in `appsettings.Development.json` (gitignored). Dual connection strings:

- `umbracoDbDSN` — CMS database
- `NimbusBoard` — product database

### SMTP (optional)

In `appsettings.json`:

```json
"Smtp": {
  "Enabled": false,
  "Host": "localhost",
  "Port": 25,
  "From": "nimbus@localhost",
  "UseSsl": false
}
```

When `Enabled` is `false`, emails are written to the application log.

## Tests

```bash
dotnet test NimbusBoard.slnx
```

## Docs

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — layering and Umbraco integration
- [docs/API-FLOWS.md](docs/API-FLOWS.md) — primary request flows
- [src/Packaging/README.md](src/Packaging/README.md) — publish notes

## Screenshots

Capture after running locally:

1. `/app/dashboard` — KPIs, urgent list, burndown
2. `/app/boards/{id}` — Kanban columns
3. `/app/sprints/{id}` — sprint detail + chart
