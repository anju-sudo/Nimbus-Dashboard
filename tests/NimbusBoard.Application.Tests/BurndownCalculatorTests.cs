using FluentAssertions;
using NimbusBoard.Application.Common;
using Xunit;

namespace NimbusBoard.Application.Tests;

public class BurndownCalculatorTests
{
    [Fact]
    public void Ideal_at_start_equals_total()
    {
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 15);
        BurndownCalculator.CalculateIdealPoints(20, start, end, start).Should().Be(20);
    }

    [Fact]
    public void Ideal_at_end_is_zero()
    {
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 15);
        BurndownCalculator.CalculateIdealPoints(20, start, end, end).Should().Be(0);
    }

    [Fact]
    public void Ideal_midpoint_is_roughly_half()
    {
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 15);
        var mid = new DateTime(2026, 7, 8);
        var ideal = BurndownCalculator.CalculateIdealPoints(20, start, end, mid);
        ideal.Should().BeInRange(8, 12);
    }

    [Fact]
    public void Remaining_excludes_done_issues()
    {
        var remaining = BurndownCalculator.CalculateRemainingPoints(
        [
            (5, false),
            (3, true),
            (8, false)
        ]);
        remaining.Should().Be(13);
    }

    [Fact]
    public void BuildIdealSeries_covers_each_day()
    {
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 3);
        var series = BurndownCalculator.BuildIdealSeries(10, start, end);
        series.Should().HaveCount(3);
        series[0].Ideal.Should().Be(10);
        series[^1].Ideal.Should().Be(0);
    }
}
