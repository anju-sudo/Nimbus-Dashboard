# Architecture

Nimbus Board follows a Clean Architecture-style layout under `src/`.

```text
src/
  Domain/NimbusBoard.Domain
    └── Entities + Enums (+ BaseEntity)

  Core/NimbusBoard.Application
    ├── *Commands / *Queries / Handlers   MediatR use cases
    ├── Common (BurndownCalculator, IssueStatusStateMachine)
    └── Interfaces (INimbusBoardDbContext, IBurndownService, IEmailSender, ...)

  Infrastructure/NimbusBoard.Infrastructure
    ├── Persistence/NimbusBoardDbContext
    └── Services (BurndownService, SmtpEmailSender, NotificationPublisher, IssueKeyFactory)

  Host/NimbusBoard.Web   (Umbraco entry + Presentation)
    ├── Pages/App/*          Razor Pages UI + HTMX endpoints
    ├── Hubs/NotificationHub SignalR
    ├── Services/*           Host adapters (SignalR publisher, media storage)
    └── Composers/*          DI wiring

  Packaging/                 Publish / deploy notes

tests/NimbusBoard.Application.Tests
docs/                        ARCHITECTURE + API-FLOWS
```

**Dependency rule:** Host → Infrastructure → Core → Domain. Core never references Infrastructure or Host.

> Why no separate Presentation project? Umbraco owns the web pipeline. Razor Pages, hubs, and composers stay in Host so CMS and `/app` UI share one startup.

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
- Issue assignee picker uses project `ProjectMember` list

## Auth model (demo)

Pages currently operate as seeded member **Anjumol Babu** (`MemberId = 1`). Umbraco member auth is available for CMS; product pages use the seeded project member id for notifications and My Work.
