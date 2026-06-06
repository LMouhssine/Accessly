using Accessly.Domain.Common;
using FluentAssertions;
using Xunit;

namespace Accessly.UnitTests.Domain;

public class CodeGeneratorTests
{
    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(20)]
    public void Generate_returns_requested_length(int length)
    {
        CodeGenerator.Generate(length).Should().HaveLength(length);
    }

    [Fact]
    public void Generate_uses_only_unambiguous_characters()
    {
        CodeGenerator.Generate(500).Should().MatchRegex("^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]+$");
    }

    [Fact]
    public void Generate_produces_distinct_values()
    {
        var codes = Enumerable.Range(0, 1000).Select(_ => CodeGenerator.Generate()).ToHashSet();
        codes.Count.Should().Be(1000);
    }

    [Fact]
    public void Generate_throws_for_non_positive_length()
    {
        var act = () => CodeGenerator.Generate(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
