# Architecture

Nimbus Board is a clean-ish layered ASP.NET Core app hosted inside Umbraco.

```text
Nimbus Board (Host / Umbraco)
  ├── Pages/App/*          Razor Pages UI + HTMX endpoints
  ├── Hubs/NotificationHub SignalR
  ├── Services/*           Host adapters (SignalR publisher, media storage)
  └── Composers/*          DI wiring

src/NimbusBoard.Application
  ├── *Commands / *Queries / Handlers  MediatR
  ├── Common (BurndownCalculator, IssueStatusStateMachine)
  └── Interfaces (INimbusBoardDbContext, IBurndownService, IEmailSender, ...)

src/NimbusBoard.Infrastructure
  ├── Persistence/NimbusBoardDbContext
  └── Services (BurndownService, SmtpEmailSender, NotificationPublisher, IssueKeyFactory)

src/NimbusBoard.Domain
  └── Entities + Enums
```

## Dual databases

| Database | Purpose |
|---|---|
| Umbraco SQLite (`umbracoDbDSN`) | CMS, members, media library |
| NimbusBoard SQLite (`NimbusBoard`) | Projects, issues, boards, sprints, notifications, activity |

`NimbusBoardComposer` registers Infrastructure + SignalR + Razor Pages. On startup, `EnsureNimbusBoardDatabaseAsync()` creates/seeds the product DB.

## Cross-cutting services

- **BurndownCalculator** — pure ideal/remaining math (unit-tested)
- **BurndownService** — recalculates sprint points and upserts daily `BurndownSnapshot` rows
- **IAppNotificationService** — persists `Notification`, optionally emails, then pushes SignalR (`SignalRNotificationPublisher` in Host)
- **IEmailSender / SmtpEmailSender** — SMTP when `Smtp:Enabled`, otherwise logs
- **IssueKeyFactory** — `{ProjectKey}-{Counter}` (e.g. `NIM-105`)
- **IssueStatusStateMachine** — validates status transitions on board moves

## UI composition

- Shared layout: `Pages/App/Shared/_AppLayout.cshtml` (responsive sidebar, ⌘K search, SignalR client)
- Dashboard aggregates via `GetDashboardQuery`
- Boards use Sortable.js → `MoveIssueCommand`
- Comments/labels/attachments use HTMX partials

## Auth model (demo)

Pages currently operate as seeded member **Anjumol Babu** (`MemberId = 1`). Umbraco member auth is available for CMS; product pages use the seeded project member id for notifications and My Work.
