using System.Text.Json;
using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Entities;

namespace Accessly.Infrastructure.Auditing;

public sealed class AuditLogger(IAppDbContext db, ICurrentUser user, IClock clock) : IAuditLogger
{
    public async Task LogAsync(string action, string entityType, string? entityId = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLog
        {
            Action = action,
            ActorId = user.UserId,
            OrganizationId = user.OrganizationId,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = clock.UtcNow,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata),
        };

        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }
}
