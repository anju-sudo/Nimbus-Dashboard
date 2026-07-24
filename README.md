# Nimbus Board

Lightweight Jira alternative built on **Umbraco CMS**, **EF Core**, **MediatR**, **HTMX**, **SignalR**, and **Chart.js**.

## Features

- Dashboard with KPI cards, urgent tasks, sprint preview, and burndown chart
- Projects, issues (NIM-### keys), and Kanban boards with drag-and-drop
- Comments, labels, attachments, and activity log
- Sprint planning (create / start / complete / assign) with live burndown snapshots
- In-app notifications with SignalR badge updates and optional SMTP email
- Global search via **⌘K** / **Ctrl+K**

## Stack

| Layer | Tech |
|---|---|
| Host | ASP.NET Core + Umbraco 17 Razor Pages (`/app/*`) |
| Application | MediatR commands/queries |
| Infrastructure | EF Core SQLite (`NimbusBoard` DB) |
| Auth / CMS | Umbraco members + separate Umbraco SQLite |
| UI | Tailwind CDN, HTMX, Chart.js, Sortable.js, SignalR |

## Run locally

```bash
dotnet restore NimbusBoard.slnx
dotnet run --project "Nimbus Board/Nimbus Board.csproj"
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

- [ARCHITECTURE.md](ARCHITECTURE.md) — layering and Umbraco integration
- [API-FLOWS.md](API-FLOWS.md) — primary request flows

## Screenshots

Capture after running locally:

1. `/app/dashboard` — KPIs, urgent list, burndown
2. `/app/boards/{id}` — Kanban columns
3. `/app/sprints/{id}` — sprint detail + chart
