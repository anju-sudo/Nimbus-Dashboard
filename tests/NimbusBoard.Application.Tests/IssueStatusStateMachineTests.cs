using FluentAssertions;
using NimbusBoard.Application.Common;
using NimbusBoard.Domain.Enums;
using Xunit;

namespace NimbusBoard.Application.Tests;

public class IssueStatusStateMachineTests
{
    [Theory]
    [InlineData(IssueStatus.ToDo, IssueStatus.InProgress, true)]
    [InlineData(IssueStatus.InProgress, IssueStatus.Done, true)]
    [InlineData(IssueStatus.Backlog, IssueStatus.Done, false)]
    [InlineData(IssueStatus.Done, IssueStatus.ToDo, true)]
    [InlineData(IssueStatus.ToDo, IssueStatus.ToDo, true)]
    public void CanTransition(IssueStatus from, IssueStatus to, bool expected)
    {
        IssueStatusStateMachine.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void EnsureCanTransition_throws_for_illegal_move()
    {
        var act = () => IssueStatusStateMachine.EnsureCanTransition(IssueStatus.Backlog, IssueStatus.Done);
        act.Should().Throw<InvalidOperationException>();
    }
}
