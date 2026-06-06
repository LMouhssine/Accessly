using Accessly.Domain.Enums;

namespace Accessly.Application.Features.CheckIns;

public sealed record CheckInDto(
    Guid Id,
    Guid TicketId,
    string TicketCode,
    string AttendeeName,
    DateTimeOffset CheckedInAt,
    CheckInResult Result);

public sealed record CheckInSummaryDto(
    Guid EventId,
    int Registered,
    int CheckedIn,
    int Capacity,
    double FillRate,
    IReadOnlyList<CheckInDto> Recent);

public sealed record CheckInResponse(CheckInResult Result, bool Accepted, string Message, CheckInDto? CheckIn);
