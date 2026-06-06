using Accessly.Application.Common.Interfaces;
using Accessly.Infrastructure.Ai;
using FluentAssertions;
using Xunit;

namespace Accessly.UnitTests.Features;

public class FakeAiProviderTests
{
    private readonly FakeAiProvider _provider = new();

    [Fact]
    public async Task Description_is_deterministic_and_mentions_the_title()
    {
        var context = new AiEventContext("Cloud Summit", "Conference", "developers");

        var first = await _provider.GenerateEventDescriptionAsync(context);
        var second = await _provider.GenerateEventDescriptionAsync(context);

        first.Should().Be(second);
        first.Should().Contain("Cloud Summit");
    }

    [Fact]
    public async Task Tags_are_lowercase_and_non_empty()
    {
        var tags = await _provider.SuggestTagsAsync(new AiEventContext("Cloud Native Summit", "Conference", Topics: "kubernetes, observability"));

        tags.Should().NotBeEmpty();
        tags.Should().OnlyContain(tag => tag == tag.ToLowerInvariant());
    }

    [Fact]
    public async Task Feedback_summary_reports_count_and_title()
    {
        var feedback = new List<FeedbackSnippet> { new(5, "Great"), new(4, null), new(2, "Too long") };

        var summary = await _provider.SummarizeFeedbackAsync("Cloud Summit", feedback);

        summary.Should().Contain("3 responses");
        summary.Should().Contain("Cloud Summit");
    }

    [Fact]
    public async Task Feedback_summary_handles_no_feedback()
    {
        var summary = await _provider.SummarizeFeedbackAsync("Cloud Summit", []);

        summary.Should().Contain("No feedback");
    }
}
