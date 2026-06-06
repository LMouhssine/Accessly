using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.CheckIns;

public sealed record RecordCheckInCommand(string Code, Guid EventId) : ICommand<CheckInResponse>;

public sealed class RecordCheckInValidator : AbstractValidator<RecordCheckInCommand>
{
    public RecordCheckInValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EventId).NotEmpty();
    }
}

public sealed class RecordCheckInHandler(
    IAppDbContext db,
    ICurrentUser user,
    IClock clock,
    IAuditLogger audit,
    ICheckInNotifier notifier) : IRequestHandler<RecordCheckInCommand, CheckInResponse>
{
    public async Task<CheckInResponse> Handle(RecordCheckInCommand request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);
        AccessGuard.EnsureCanOperateEvent(user, @event.OrganizationId);

        var code = request.Code.Trim().ToUpperInvariant();
        var ticket = await db.Tickets
            .Include(t => t.Booking).ThenInclude(b => b.AttendeeUser)
            .FirstOrDefaultAsync(t => t.Code == code, cancellationToken);

        if (ticket is null)
        {
            return new CheckInResponse(CheckInResult.InvalidTicket, false, "Ticket not found.", null);
        }

        var result = DetermineResult(ticket, request.EventId);
        var now = clock.UtcNow;

        if (result == CheckInResult.Accepted)
        {
            ticket.Status = TicketStatus.Used;
            ticket.UsedAt = now;
        }

        var checkIn = new CheckIn
        {
            TicketId = ticket.Id,
            EventId = request.EventId,
            StaffUserId = user.UserId,
            CheckedInAt = now,
            Result = result,
        };
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.CheckInRecorded, nameof(CheckIn), checkIn.Id.ToString(), new { ticket.Code, Result = result.ToString() }, cancellationToken);

        var dto = new CheckInDto(checkIn.Id, ticket.Id, ticket.Code, ticket.Booking.AttendeeUser.DisplayName, now, result);
        var summary = await CheckInSummaryBuilder.BuildAsync(db, @event, 10, cancellationToken);
        await notifier.CheckInRecordedAsync(@event.Id, summary, dto, cancellationToken);

        return new CheckInResponse(result, result == CheckInResult.Accepted, MessageFor(result), dto);
    }

    private static CheckInResult DetermineResult(Ticket ticket, Guid eventId)
    {
        if (ticket.EventId != eventId)
        {
            return CheckInResult.EventMismatch;
        }

        return ticket.Status switch
        {
            TicketStatus.Cancelled => CheckInResult.TicketCancelled,
            TicketStatus.Used => CheckInResult.AlreadyCheckedIn,
            _ => CheckInResult.Accepted,
        };
    }

    private static string MessageFor(CheckInResult result) => result switch
    {
        CheckInResult.Accepted => "Check-in accepted.",
        CheckInResult.AlreadyCheckedIn => "This ticket has already been checked in.",
        CheckInResult.TicketCancelled => "This ticket has been cancelled.",
        CheckInResult.EventMismatch => "This ticket is for a different event.",
        _ => "Invalid ticket.",
    };
}
