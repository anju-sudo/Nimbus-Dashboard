# Architecture

Nimbus Board follows a Clean Architecture-style layout under `src/`.

```text
src/
  Domain/NimbusBoard.Domain
    └── Entities + Enums (+ abstract BaseEntity)

  Core/NimbusBoard.Application
    ├── Feature folders (Commands / Queries / Handlers / Models)
    ├── Common (BurndownCalculator, IssueStatusStateMachine, ports)
    └── Interfaces (INimbusBoardDbContext, IBurndownService, IEmailSender, …)

  Infrastructure/NimbusBoard.Infrastructure
    ├── Persistence/ (+ Seeding/)
    ├── Storage/          LocalFileAttachmentStorage
    ├── Email/            SmtpEmailSender + SmtpOptions
    ├── Notifications/    NotificationPublisher (persist + email)
    ├── Burndown/
    └── Identity/         IssueKeyFactory

  Host/NimbusBoard.Web   (Umbraco entry + Presentation)
    ├── Pages/App/* + Views/*     Razor / CMS templates
    ├── Hubs/NotificationHub      SignalR transport
    ├── Services/*                Umbraco media + SignalR decorator only
    ├── Notifications/*           Umbraco content-type seed handlers
    ├── Models/*                  CMS presentation DTOs (DashboardCopy)
    └── Composers/*               Host DI overrides
```

**Dependency rule:** Host → Infrastructure → Core → Domain. Core never references Infrastructure or Host.

## What belongs where

| Concern | Layer |
|---|---|
| Entities, enums | Domain |
| MediatR use cases, ports | Application (Core) |
| EF, SMTP, local files, seed | Infrastructure |
| Umbraco, Razor Pages, SignalR hub, CMS seed | Host |

> Why no separate Presentation project? Umbraco owns the web pipeline. Razor Pages, hubs, and composers stay in Host so CMS and `/app` UI share one startup.

## Dual databases

| Database | Purpose |
|---|---|
| Umbraco SQLite (`umbracoDbDSN`) | CMS, members, media library |
| NimbusBoard SQLite (`NimbusBoard`) | Projects, issues, boards, sprints, notifications, activity |

## Host adapter registration

`NimbusBoardComposer` calls `AddNimbusBoardInfrastructure`, then overrides:

- `IAttachmentStorage` → `UmbracoMediaAttachmentAdapter` (falls back to Infra `LocalFileAttachmentStorage`)
- `IAppNotificationService` → `SignalRNotificationPublisher` decorating Infra `NotificationPublisher`
- `AttachmentStorageOptions.RootPath` → `{WebRoot}/nimbus-uploads`

Pages talk only to `IMediator` (plus Umbraco published content for CMS-editable dashboard labels on `/`).

## Cross-cutting services

- **BurndownCalculator** — pure ideal/remaining math (unit-tested)
- **BurndownService** — recalculates sprint points and upserts daily `BurndownSnapshot` rows
- **IAppNotificationService** — persists `Notification`, optionally emails; Host decorator adds SignalR
- **IEmailSender / SmtpEmailSender** — SMTP when `Smtp:Enabled`, otherwise logs
- **IssueKeyFactory** — `{ProjectKey}-{Counter}` (e.g. `NIM-105`)
- **IssueStatusStateMachine** — validates status transitions on board moves

## UI composition

- Shared layout: `Pages/App/Shared/_AppLayout.cshtml`
- Home template (`Views/Home.cshtml`) renders the dashboard at `/` with CMS copy
- Boards use Sortable.js → `MoveIssueCommand`
- Comments/labels/attachments use HTMX partials

## Auth model (demo)

Pages currently operate as seeded member **Anjumol Babu** (`MemberId = 1`).
