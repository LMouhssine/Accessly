using System.Globalization;
using System.Text;
using Accessly.Application.Common.Interfaces;

namespace Accessly.Infrastructure.Ai;

/// <summary>Deterministic, offline assistant used by default. Produces stable output for demos and tests.</summary>
public sealed class FakeAiProvider : IAiProvider
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "your", "you", "our", "a", "an", "of", "to", "in", "on", "at",
    };

    public string Name => "Fake";

    public Task<string> GenerateEventDescriptionAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var category = string.IsNullOrWhiteSpace(context.Category) ? "event" : context.Category!.ToLowerInvariant();
        var audience = string.IsNullOrWhiteSpace(context.Audience) ? "attendees" : context.Audience;

        var builder = new StringBuilder();
        builder.Append($"Join us for {context.Title}, a {category} crafted for {audience}. ");
        builder.Append($"Across the {category} you'll gain practical insights, connect with peers, and leave with concrete takeaways. ");
        if (!string.IsNullOrWhiteSpace(context.Topics))
        {
            builder.Append($"Sessions cover {context.Topics}. ");
        }

        if (!string.IsNullOrWhiteSpace(context.VenueName))
        {
            builder.Append($"It all happens at {context.VenueName}. ");
        }

        builder.Append("Reserve your place today.");
        return Task.FromResult(builder.ToString());
    }

    public Task<IReadOnlyList<string>> SuggestTagsAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var tags = new List<string>();
        void Add(string tag)
        {
            var normalized = tag.Trim().ToLowerInvariant();
            if (normalized.Length > 2 && !StopWords.Contains(normalized) && !tags.Contains(normalized))
            {
                tags.Add(normalized);
            }
        }

        if (!string.IsNullOrWhiteSpace(context.Category))
        {
            Add(context.Category!);
        }

        foreach (var word in Tokenize(context.Title))
        {
            if (tags.Count >= 6)
            {
                break;
            }

            Add(word);
        }

        foreach (var word in Tokenize(context.Topics))
        {
            if (tags.Count >= 8)
            {
                break;
            }

            Add(word);
        }

        if (tags.Count == 0)
        {
            tags.Add("event");
        }

        return Task.FromResult<IReadOnlyList<string>>(tags);
    }

    public Task<string> GenerateReminderEmailAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var when = context.StartAt is { } start
            ? start.ToString("dddd, dd MMMM yyyy 'at' HH:mm", CultureInfo.InvariantCulture)
            : "soon";
        var venue = string.IsNullOrWhiteSpace(context.VenueName) ? "the venue" : context.VenueName;

        var body = $"Subject: Reminder — {context.Title} is almost here\n\n" +
                   $"Hello,\n\n" +
                   $"This is a friendly reminder that {context.Title} takes place {when} at {venue}. " +
                   "Please have your QR code ticket ready for a smooth check-in.\n\n" +
                   "We look forward to seeing you there.\n\n" +
                   "— The Accessly team";
        return Task.FromResult(body);
    }

    public Task<string> GenerateAgendaAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var hours = Math.Clamp(context.DurationHours ?? 3, 1, 8);
        var topics = Tokenize(context.Topics).ToList();
        var builder = new StringBuilder();
        builder.AppendLine($"Proposed agenda for {context.Title} ({hours}h):");

        var slotStart = 9 * 60;
        builder.AppendLine($"- {FormatTime(slotStart)}: Welcome and introductions");
        slotStart += 30;
        for (var i = 0; i < hours && slotStart < (9 * 60) + (hours * 60); i++)
        {
            var topic = topics.Count > 0 ? topics[i % topics.Count] : $"session {i + 1}";
            builder.AppendLine($"- {FormatTime(slotStart)}: Deep dive — {topic}");
            slotStart += 45;
            builder.AppendLine($"- {FormatTime(slotStart)}: Break and networking");
            slotStart += 15;
        }

        builder.Append($"- {FormatTime(slotStart)}: Closing remarks and next steps");
        return Task.FromResult(builder.ToString());
    }

    public Task<string> GenerateAnnouncementAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var category = string.IsNullOrWhiteSpace(context.Category) ? "event" : context.Category!.ToLowerInvariant();
        var text = $"📣 Announcing {context.Title}! We're excited to host this {category}" +
                   (string.IsNullOrWhiteSpace(context.VenueName) ? string.Empty : $" at {context.VenueName}") +
                   ". Spots are limited — book your ticket now and be part of it.";
        return Task.FromResult(text);
    }

    public Task<string> SummarizeFeedbackAsync(string eventTitle, IReadOnlyList<FeedbackSnippet> feedback, CancellationToken cancellationToken = default)
    {
        if (feedback.Count == 0)
        {
            return Task.FromResult($"No feedback has been submitted for {eventTitle} yet.");
        }

        var average = Math.Round(feedback.Average(f => f.Rating), 2);
        var positive = feedback.Count(f => f.Rating >= 4);
        var negative = feedback.Count(f => f.Rating <= 2);
        var sentiment = average >= 4 ? "very positive" : average >= 3 ? "generally positive" : "mixed";

        var comments = feedback
            .Where(f => !string.IsNullOrWhiteSpace(f.Comment))
            .Select(f => f.Comment!.Trim())
            .Take(3)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"Feedback summary for {eventTitle}:");
        builder.AppendLine($"- {feedback.Count} responses, average rating {average}/5 ({sentiment}).");
        builder.AppendLine($"- {positive} positive (4-5★) and {negative} critical (1-2★) responses.");
        if (comments.Count > 0)
        {
            builder.AppendLine("- Representative comments:");
            foreach (var comment in comments)
            {
                builder.AppendLine($"  • \"{comment}\"");
            }
        }

        return Task.FromResult(builder.ToString().TrimEnd());
    }

    private static IEnumerable<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var raw in text.Split([' ', ',', ';', '.', '/', '-', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            var word = new string(raw.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            if (word.Length > 2 && !StopWords.Contains(word))
            {
                yield return word;
            }
        }
    }

    private static string FormatTime(int minutesFromMidnight)
    {
        var hours = (minutesFromMidnight / 60) % 24;
        var minutes = minutesFromMidnight % 60;
        return $"{hours:D2}:{minutes:D2}";
    }
}
