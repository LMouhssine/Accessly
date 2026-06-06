using Accessly.Domain.Enums;

namespace Accessly.Application.Features.Events;

public sealed record SpeakerDto(Guid Id, string Name, string? Title, string? Bio);

public sealed record SpeakerInput(string Name, string? Title, string? Bio);

public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string? Category,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string VenueName,
    int Capacity,
    int BookedCount,
    decimal PriceAmount,
    string Currency,
    EventStatus Status,
    string? CoverImageUrl,
    Guid OrganizationId);

public sealed record EventDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string? Category,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string VenueName,
    string? VenueAddress,
    int Capacity,
    int BookedCount,
    decimal PriceAmount,
    string Currency,
    EventStatus Status,
    string? CoverImageUrl,
    Guid OrganizationId,
    IReadOnlyList<SpeakerDto> Speakers);
