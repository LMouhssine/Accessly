using Accessly.Application.Features.Audit;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Accessly.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace Accessly.UnitTests.Features;

public class GetAuditLogsHandlerTests
{
    [Fact]
    public async Task Organizer_only_sees_own_organization_entries()
    {
        await using var db = TestDb.NewInMemory();
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();
        db.AuditLogs.AddRange(
            NewLog("event.created", orgId),
            NewLog("event.published", orgId),
            NewLog("event.created", otherOrgId));
        await db.SaveChangesAsync();

        var handler = new GetAuditLogsHandler(db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Organizer, orgId));

        var result = await handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(a => a.OrganizationId == orgId);
    }

    [Fact]
    public async Task Admin_sees_all_organizations()
    {
        await using var db = TestDb.NewInMemory();
        db.AuditLogs.AddRange(
            NewLog("event.created", Guid.NewGuid()),
            NewLog("event.created", Guid.NewGuid()));
        await db.SaveChangesAsync();

        var handler = new GetAuditLogsHandler(db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Admin, null));

        var result = await handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Filters_by_action_and_returns_newest_first()
    {
        await using var db = TestDb.NewInMemory();
        var orgId = Guid.NewGuid();
        db.AuditLogs.AddRange(
            NewLog("event.created", orgId, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            NewLog("event.published", orgId, new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)),
            NewLog("event.published", orgId, new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero)));
        await db.SaveChangesAsync();

        var handler = new GetAuditLogsHandler(db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Admin, null));

        var result = await handler.Handle(new GetAuditLogsQuery(Action: "event.published"), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Timestamp.Should().BeAfter(result.Items[1].Timestamp);
    }

    [Fact]
    public async Task Paging_clamps_page_size_and_reports_total()
    {
        await using var db = TestDb.NewInMemory();
        var orgId = Guid.NewGuid();
        for (var i = 0; i < 5; i++)
        {
            db.AuditLogs.Add(NewLog("event.created", orgId));
        }

        await db.SaveChangesAsync();

        var handler = new GetAuditLogsHandler(db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Admin, null));

        var result = await handler.Handle(new GetAuditLogsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.TotalPages.Should().Be(3);
    }

    private static AuditLog NewLog(string action, Guid organizationId, DateTimeOffset? timestamp = null) => new()
    {
        Action = action,
        EntityType = "Event",
        OrganizationId = organizationId,
        ActorId = Guid.NewGuid(),
        Timestamp = timestamp ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
    };
}
