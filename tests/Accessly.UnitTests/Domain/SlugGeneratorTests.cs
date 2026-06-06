using Accessly.Domain.Common;
using FluentAssertions;
using Xunit;

namespace Accessly.UnitTests.Domain;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Tech Conference 2026", "tech-conference-2026")]
    [InlineData("  Hello   World  ", "hello-world")]
    [InlineData("Café & Croissants!", "cafe-croissants")]
    [InlineData("Already-Slugged", "already-slugged")]
    [InlineData("Multiple---Hyphens", "multiple-hyphens")]
    public void Generate_produces_url_friendly_slug(string input, string expected)
    {
        SlugGenerator.Generate(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Generate_returns_empty_for_blank_input(string? input)
    {
        SlugGenerator.Generate(input!).Should().BeEmpty();
    }
}
