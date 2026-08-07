using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Infrastructure.Persistence;
using NimbusBoard.Infrastructure.Services;
using Xunit;

namespace NimbusBoard.Application.Tests;

public class IssueKeyFactoryTests
{
    [Fact]
    public async Task CreateNextKey_increments_counter_and_formats_key()
    {
        await using var db = CreateDb();
        var project = new Project { Key = "NIM", Name = "Nimbus", IssueCounter = 100 };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var factory = new IssueKeyFactory(db);
        var (number, key) = await factory.CreateNextKeyAsync(project);

        number.Should().Be(101);
        key.Should().Be("NIM-101");
        project.IssueCounter.Should().Be(101);
    }

    private static NimbusBoardDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NimbusBoardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NimbusBoardDbContext(options);
    }
}
