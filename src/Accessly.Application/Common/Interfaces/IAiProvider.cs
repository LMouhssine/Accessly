namespace Accessly.Application.Common.Interfaces;

/// <summary>Context describing an event for the assistant to work from.</summary>
public sealed record AiEventContext(
    string Title,
    string? Category = null,
    string? Audience = null,
    string? Topics = null,
    DateTimeOffset? StartAt = null,
    string? VenueName = null,
    int? DurationHours = null);

public sealed record FeedbackSnippet(int Rating, string? Comment);

/// <summary>
/// Event assistant provider. The default implementation is deterministic and offline; an
/// optional OpenAI-compatible provider can be enabled through configuration.
/// </summary>
public interface IAiProvider
{
    string Name { get; }

    Task<string> GenerateEventDescriptionAsync(AiEventContext context, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SuggestTagsAsync(AiEventContext context, CancellationToken cancellationToken = default);

    Task<string> GenerateReminderEmailAsync(AiEventContext context, CancellationToken cancellationToken = default);

    Task<string> GenerateAgendaAsync(AiEventContext context, CancellationToken cancellationToken = default);

    Task<string> GenerateAnnouncementAsync(AiEventContext context, CancellationToken cancellationToken = default);

    Task<string> SummarizeFeedbackAsync(string eventTitle, IReadOnlyList<FeedbackSnippet> feedback, CancellationToken cancellationToken = default);
}
