namespace Accessly.Application.Features.Feedbacks;

public sealed record FeedbackDto(Guid Id, Guid EventId, int Rating, string? Comment, DateTimeOffset CreatedAt);

public sealed record FeedbackListDto(Guid EventId, int Count, double AverageRating, IReadOnlyList<FeedbackDto> Items);
