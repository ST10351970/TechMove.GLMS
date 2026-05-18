using FluentAssertions;
using TechMove.GLMS.Core.Services.Strategies;
using Xunit;

namespace TechMove.GLMS.Tests.Strategies;

public class CurrencyStrategyResolverTests
{
    private CurrencyStrategyResolver CreateResolver() =>
        new(new ICurrencyStrategy[]
        {
            new UsdToZarStrategy(),
            new EurToZarStrategy(),
            new GbpToZarStrategy()
        });

    [Fact]
    public void Resolve_KnownCurrency_ReturnsCorrectStrategy()
    {
        var resolver = CreateResolver();
        var strategy = resolver.Resolve("USD");
        strategy.Should().BeOfType<UsdToZarStrategy>();
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var resolver = CreateResolver();
        resolver.Resolve("usd").Should().BeOfType<UsdToZarStrategy>();
        resolver.Resolve("Eur").Should().BeOfType<EurToZarStrategy>();
        resolver.Resolve("GBP").Should().BeOfType<GbpToZarStrategy>();
    }

    [Fact]
    public void Resolve_UnknownCurrency_ThrowsNotSupportedException()
    {
        var resolver = CreateResolver();
        Action act = () => resolver.Resolve("JPY");
        act.Should().Throw<NotSupportedException>()
           .WithMessage("*JPY*");
    }

    [Fact]
    public void Resolve_NullOrEmptyCode_ThrowsArgumentException()
    {
        var resolver = CreateResolver();
        Action actNull = () => resolver.Resolve(null!);
        Action actEmpty = () => resolver.Resolve("");

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }
}