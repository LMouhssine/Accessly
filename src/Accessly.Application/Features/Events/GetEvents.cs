using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Events;

public sealed record GetEventsQuery(
    int Page = 1,
    int PageSize = 12,
    EventStatus? Status = null,
    string? Search = null,
    string? Category = null,
    bool PublishedOnly = true,
    Guid? OrganizationId = null) : IQuery<PagedResult<EventSummaryDto>>;

public sealed class GetEventsHandler(IAppDbContext db)
    : IRequestHandler<GetEventsQuery, PagedResult<EventSummaryDto>>
{
    public async Task<PagedResult<EventSummaryDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Events.AsNoTracking();

        if (request.PublishedOnly)
        {
            query = query.Where(e => e.Status == EventStatus.Published);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(e => e.OrganizationId == request.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(e => e.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(e => EF.Functions.Like(e.Title, $"%{term}%") || EF.Functions.Like(e.Description, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventSummaryDto(
                e.Id,
                e.Title,
                e.Slug,
                e.Category,
                e.StartAt,
                e.EndAt,
                e.VenueName,
                e.Capacity,
                e.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                e.PriceAmount,
                e.Currency,
                e.Status,
                e.CoverImageUrl,
                e.OrganizationId))
            .ToListAsync(cancellationToken);

        return new PagedResult<EventSummaryDto>(items, page, pageSize, total);
    }
}
