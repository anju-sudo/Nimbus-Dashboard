using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Sprints.Commands;
using NimbusBoard.Application.Sprints.Handlers;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;
using NimbusBoard.Infrastructure.Services;
using NSubstitute;
using Xunit;

namespace NimbusBoard.Application.Tests;

public class StartSprintCommandHandlerTests
{
    [Fact]
    public async Task StartSprint_activates_sprint_and_notifies_members()
    {
        await using var db = CreateDb();
        var project = new Project { Key = "NIM", Name = "Nimbus" };
        var sprint = new Sprint
        {
            Project = project,
            Name = "Sprint 1",
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(13),
            IsActive = false,
            TotalStoryPoints = 5
        };
        sprint.Issues.Add(new Issue
        {
            Project = project,
            Key = "NIM-1",
            Title = "Task",
            Number = 1,
            StoryPoints = 5,
            Status = IssueStatus.ToDo
        });
        db.ProjectMembers.Add(new ProjectMember
        {
            Project = project,
            MemberId = 1,
            DisplayName = "Anjumol Babu",
            Initials = "AB",
            Role = ProjectRole.Admin
        });
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var notifications = Substitute.For<IAppNotificationService>();
        var burndown = new BurndownService(db);
        var handler = new StartSprintCommandHandler(db, burndown, notifications);

        await handler.Handle(new StartSprintCommand(sprint.Id), CancellationToken.None);

        (await db.Sprints.SingleAsync(s => s.Id == sprint.Id)).IsActive.Should().BeTrue();
        await notifications.Received().PublishAsync(
            1,
            NotificationType.SprintStarted,
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static NimbusBoardDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NimbusBoardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NimbusBoardDbContext(options);
    }
}

public class MoveIssueCommandHandlerTests
{
    [Fact]
    public async Task MoveIssue_updates_status_and_takes_burndown_snapshot()
    {
        await using var db = CreateDb();
        var project = new Project { Key = "NIM", Name = "Nimbus" };
        var board = new Board { Project = project, Name = "Kanban" };
        var todo = new BoardColumn { Board = board, Name = "To Do", SortOrder = 1 };
        var done = new BoardColumn { Board = board, Name = "Done", SortOrder = 2 };
        board.Columns.Add(todo);
        board.Columns.Add(done);
        var sprint = new Sprint
        {
            Project = project,
            Name = "Sprint 1",
            StartDate = DateTime.UtcNow.Date.AddDays(-2),
            EndDate = DateTime.UtcNow.Date.AddDays(12),
            IsActive = true,
            TotalStoryPoints = 5
        };
        var issue = new Issue
        {
            Project = project,
            BoardColumn = todo,
            Sprint = sprint,
            Key = "NIM-2",
            Title = "Move me",
            Number = 2,
            StoryPoints = 5,
            Status = IssueStatus.ToDo,
            AssigneeMemberId = 2,
            AssigneeName = "Jane"
        };
        db.Boards.Add(board);
        db.BoardColumns.AddRange(todo, done);
        db.Issues.Add(issue);
        await db.SaveChangesAsync();

        var notifications = Substitute.For<IAppNotificationService>();
        var burndown = new BurndownService(db);
        var handler = new Application.Issues.Handlers.MoveIssueCommandHandler(db, burndown, notifications);

        await handler.Handle(new Application.Issues.Commands.MoveIssueCommand(issue.Id, done.Id), CancellationToken.None);

        var updated = await db.Issues.SingleAsync(i => i.Id == issue.Id);
        updated.Status.Should().Be(IssueStatus.Done);
        (await db.BurndownSnapshots.CountAsync(s => s.SprintId == sprint.Id)).Should().BeGreaterThan(0);
        await notifications.Received().PublishAsync(
            Arg.Any<int>(),
            NotificationType.IssueMoved,
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static NimbusBoardDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NimbusBoardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NimbusBoardDbContext(options);
    }
}
