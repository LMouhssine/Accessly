using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Feedbacks;

public sealed record SubmitFeedbackCommand(Guid EventId, int Rating, string? Comment = null) : ICommand<FeedbackDto>;

public sealed class SubmitFeedbackValidator : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public sealed class SubmitFeedbackHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<SubmitFeedbackCommand, FeedbackDto>
{
    public async Task<FeedbackDto> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);

        var exists = await db.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(Event), request.EventId);
        }

        var alreadySubmitted = await db.Feedbacks.AnyAsync(
            f => f.EventId == request.EventId && f.AttendeeUserId == user.UserId, cancellationToken);
        if (alreadySubmitted)
        {
            throw new ConflictException("You have already submitted feedback for this event.");
        }

        var feedback = new Feedback
        {
            EventId = request.EventId,
            AttendeeUserId = user.UserId!.Value,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
        };

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(cancellationToken);

        return new FeedbackDto(feedback.Id, feedback.EventId, feedback.Rating, feedback.Comment, feedback.CreatedAt);
    }
}
