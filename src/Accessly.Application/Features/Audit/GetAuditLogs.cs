using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Audit;

/// <summary>A single audit-log entry projected for read APIs.</summary>
public sealed record AuditLogDto(
    Guid Id,
    string Action,
    Guid? ActorId,
    string? ActorDisplayName,
    Guid? OrganizationId,
    string EntityType,
    string? EntityId,
    DateTimeOffset Timestamp,
    string? MetadataJson);

/// <summary>
/// Lists audit-log entries newest-first with simple filtering. Organizers only see
/// their own organization; admins may see every organization.
/// </summary>
public sealed record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Action = null,
    string? EntityType = null,
    Guid? ActorId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IQuery<PagedResult<AuditLogDto>>;

public sealed class GetAuditLogsHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.AuditLogs.AsNoTracking();

        // Organizers and staff are scoped to their own organization; admins see everything.
        if (user.Role != UserRole.Admin)
        {
            var organizationId = user.OrganizationId;
            query = query.Where(a => a.OrganizationId == organizationId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var entityType = request.EntityType.Trim();
            query = query.Where(a => a.EntityType == entityType);
        }

        if (request.ActorId.HasValue)
        {
            query = query.Where(a => a.ActorId == request.ActorId.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.To.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        // Project the actor's display name with a left join so deleted users don't drop rows.
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Action,
                a.ActorId,
                db.Users.Where(u => u.Id == a.ActorId).Select(u => u.DisplayName).FirstOrDefault(),
                a.OrganizationId,
                a.EntityType,
                a.EntityId,
                a.Timestamp,
                a.MetadataJson))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, page, pageSize, total);
    }
}
