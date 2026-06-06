using Accessly.Application.Common;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Common;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Events;

public sealed record CreateEventCommand(
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
    IReadOnlyList<SpeakerInput>? Speakers = null,
    Guid? OrganizationId = null) : ICommand<Guid>;

public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.VenueName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("End time must be after the start time.");
    }
}

public sealed class CreateEventHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit)
    : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var organizationId = AccessGuard.ResolveOrganizationId(user, request.OrganizationId);
        var slug = await UniqueSlugAsync(db, organizationId, SlugGenerator.Generate(request.Title), cancellationToken);

        var @event = new Event
        {
            OrganizationId = organizationId,
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description,
            Category = request.Category,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            VenueName = request.VenueName.Trim(),
            VenueAddress = request.VenueAddress,
            Capacity = request.Capacity,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency.ToUpperInvariant(),
            CoverImageUrl = request.CoverImageUrl,
            Status = EventStatus.Draft,
        };

        if (request.Speakers is not null)
        {
            foreach (var speaker in request.Speakers)
            {
                @event.Speakers.Add(new Speaker { Name = speaker.Name, Title = speaker.Title, Bio = speaker.Bio });
            }
        }

        db.Events.Add(@event);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.EventCreated, nameof(Event), @event.Id.ToString(), new { @event.Title }, cancellationToken);

        return @event.Id;
    }

    internal static async Task<string> UniqueSlugAsync(IAppDbContext db, Guid organizationId, string baseSlug, CancellationToken cancellationToken)
    {
        var root = string.IsNullOrEmpty(baseSlug) ? "event" : baseSlug;
        var slug = root;
        var suffix = 1;
        while (await db.Events.AnyAsync(e => e.OrganizationId == organizationId && e.Slug == slug, cancellationToken))
        {
            slug = $"{root}-{++suffix}";
        }

        return slug;
    }
}
