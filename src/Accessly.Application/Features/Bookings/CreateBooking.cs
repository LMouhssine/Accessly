using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Common;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Bookings;

public sealed record CreateBookingCommand(Guid EventId, Guid? AttendeeUserId = null) : ICommand<BookingResult>;

public sealed record BookingResult(Guid BookingId, Guid TicketId, string TicketCode, BookingStatus Status);

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator() => RuleFor(x => x.EventId).NotEmpty();
}

public sealed class CreateBookingHandler(
    IAppDbContext db,
    ICurrentUser user,
    IClock clock,
    IPaymentProvider payments,
    IAuditLogger audit) : IRequestHandler<CreateBookingCommand, BookingResult>
{
    public async Task<BookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);
        var attendeeId = ResolveAttendee(request.AttendeeUserId);

        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);

        if (@event.Status != EventStatus.Published)
        {
            throw new ConflictException("Bookings are only available for published events.");
        }

        var alreadyBooked = await db.Bookings.AnyAsync(
            b => b.EventId == @event.Id
                 && b.AttendeeUserId == attendeeId
                 && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed),
            cancellationToken);
        if (alreadyBooked)
        {
            throw new ConflictException("You already have an active booking for this event.");
        }

        var confirmedCount = await db.Bookings.CountAsync(
            b => b.EventId == @event.Id && b.Status == BookingStatus.Confirmed, cancellationToken);
        if (confirmedCount >= @event.Capacity)
        {
            throw new ConflictException("This event is fully booked.");
        }

        var payment = await payments.ChargeAsync(@event.PriceAmount, @event.Currency, $"Booking for {@event.Title}", cancellationToken);
        var booking = new Booking
        {
            EventId = @event.Id,
            AttendeeUserId = attendeeId,
            Status = payment.Succeeded ? BookingStatus.Confirmed : BookingStatus.Pending,
            Payment = new Payment
            {
                Provider = payment.Provider,
                Status = payment.Status,
                Amount = @event.PriceAmount,
                Currency = @event.Currency,
                Reference = payment.Reference,
            },
        };

        if (!payment.Succeeded)
        {
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(cancellationToken);
            throw new ConflictException("Payment could not be processed.");
        }

        var code = CodeGenerator.Generate();
        booking.Ticket = new Ticket
        {
            EventId = @event.Id,
            Code = code,
            QrPayload = code,
            Status = TicketStatus.Active,
            IssuedAt = clock.UtcNow,
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.BookingCreated, nameof(Booking), booking.Id.ToString(), new { @event.Id, AttendeeId = attendeeId }, cancellationToken);

        return new BookingResult(booking.Id, booking.Ticket.Id, booking.Ticket.Code, booking.Status);
    }

    private Guid ResolveAttendee(Guid? requested)
    {
        if (requested is not null && requested != user.UserId && user.Role != UserRole.Admin)
        {
            throw new ForbiddenAccessException();
        }

        return requested ?? user.UserId!.Value;
    }
}
