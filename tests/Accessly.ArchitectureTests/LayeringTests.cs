using System.Reflection;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Common;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Accessly.ArchitectureTests;

public class LayeringTests
{
    private const string Application = "Accessly.Application";
    private const string Infrastructure = "Accessly.Infrastructure";
    private const string Api = "Accessly.Api";

    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IDispatcher).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Accessly.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(Application, Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    private static string Describe(TestResult result) =>
        "Offending types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
