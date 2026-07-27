# API / request flows

Primary flows for Nimbus Board. Most mutations go through MediatR handlers; HTMX and SignalR sit at the edges.

## Create issue

```mermaid
sequenceDiagram
  participant UI as Razor_CreateIssue
  participant MediatR
  participant KeyFactory as IssueKeyFactory
  participant DB as NimbusBoardDb
  participant Notify as AppNotificationService

  UI->>MediatR: CreateIssueCommand
  MediatR->>KeyFactory: CreateNextKeyAsync
  KeyFactory->>DB: Increment IssueCounter
  MediatR->>DB: Insert Issue + ActivityLog
  MediatR->>Notify: Assigned (if assignee set)
  Notify->>DB: Insert Notification
  Notify-->>UI: SignalR badge toast
```

## Move issue on board

```mermaid
sequenceDiagram
  participant JS as Sortable.js
  participant Page as Move_endpoint
  participant MediatR
  participant SM as IssueStatusStateMachine
  participant Burn as BurndownService
  participant Notify as AppNotificationService

  JS->>Page: POST issueId columnId
  Page->>MediatR: MoveIssueCommand
  MediatR->>SM: EnsureCanTransition
  MediatR->>MediatR: Update column + status
  MediatR->>Notify: IssueMoved
  MediatR->>Burn: Recalculate + TakeSnapshot (if in sprint)
```

## Start sprint → burndown

```mermaid
sequenceDiagram
  participant UI as Sprints_Detail
  participant MediatR
  participant Burn as BurndownService
  participant Notify as AppNotificationService
  participant DB as NimbusBoardDb

  UI->>MediatR: StartSprintCommand
  MediatR->>DB: Deactivate other sprints in project
  MediatR->>DB: Set IsActive
  MediatR->>Burn: RecalculateSprintPoints
  loop Each day start to today
    MediatR->>Burn: TakeSnapshot
  end
  loop Each project member
    MediatR->>Notify: SprintStarted
  end
```

Dashboard / sprint detail then load snapshots via `BurndownQueryHelper` → Chart.js (`burndown-chart.js`).

## Comment → notification → SignalR / email

```mermaid
sequenceDiagram
  participant UI as HTMX_Comments
  participant MediatR
  participant Notify as AppNotificationService
  participant Hub as NotificationHub
  participant Mail as SmtpEmailSender

  UI->>MediatR: AddCommentCommand
  MediatR->>MediatR: Save Comment + ActivityLog
  MediatR->>Notify: Commented
  Notify->>Notify: Persist Notification
  opt emailTo provided and type allows email
    Notify->>Mail: SendAsync
  end
  Notify->>Hub: notificationReceived to member group
  Hub-->>UI: Badge + toast
```

## Global search (⌘K)

1. Layout modal opens (`search-shortcut.js`)
2. Input `hx-get="/app/search?q=..."` (debounce 200ms)
3. `SearchQuery` matches issues (key/title), projects (key/name), boards (name)
4. Partial `_SearchResults` returns grouped links

## Umbraco member ↔ Nimbus member

```mermaid
flowchart LR
  UmbracoMember[Umbraco_Member]
  ProjectMember[ProjectMember_MemberId]
  IssueAssignee[Issue_AssigneeMemberId]
  Notification[Notification_RecipientMemberId]
  SignalRGroup[SignalR_group_member_id]

  UmbracoMember -.->|demo maps to| ProjectMember
  ProjectMember --> IssueAssignee
  ProjectMember --> Notification
  Notification --> SignalRGroup
```

In the seeded demo, member `1` (Anjumol Babu) is the active app user for My Work, badge counts, and SignalR group `member:1`. Attachments use Umbraco Media (or local file fallback) via `IAttachmentStorage`.
