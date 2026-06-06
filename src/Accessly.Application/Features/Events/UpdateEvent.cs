using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Events;

public sealed record UpdateEventCommand(
    Guid Id,
    string Title,
    string Description,
    string? Category,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string VenueName,
    string? VenueAddress,
    int Capacity,
    decimal PriceAmount,
    string Currency,
    string? CoverImageUrl = null,
    IReadOnlyList<SpeakerInput>? Speakers = null) : ICommand<Unit>;

public sealed class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.VenueName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("End time must be after the start time.");
    }
}

public sealed class UpdateEventHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit)
    : IRequestHandler<UpdateEventCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.Include(e => e.Speakers)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.Id);

        AccessGuard.EnsureCanManageOrganization(user, @event.OrganizationId);

        if (@event.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            throw new ConflictException($"A {@event.Status.ToString().ToLowerInvariant()} event cannot be edited.");
        }

        @event.Title = request.Title.Trim();
        @event.Description = request.Description;
        @event.Category = request.Category;
        @event.StartAt = request.StartAt;
        @event.EndAt = request.EndAt;
        @event.VenueName = request.VenueName.Trim();
        @event.VenueAddress = request.VenueAddress;
        @event.Capacity = request.Capacity;
        @event.PriceAmount = request.PriceAmount;
        @event.Currency = request.Currency.ToUpperInvariant();
        @event.CoverImageUrl = request.CoverImageUrl;

        if (request.Speakers is not null)
        {
            db.Speakers.RemoveRange(@event.Speakers);
            @event.Speakers.Clear();
            foreach (var speaker in request.Speakers)
            {
                @event.Speakers.Add(new Speaker { Name = speaker.Name, Title = speaker.Title, Bio = speaker.Bio });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.EventUpdated, nameof(Event), @event.Id.ToString(), null, cancellationToken);

        return Unit.Value;
    }
}
