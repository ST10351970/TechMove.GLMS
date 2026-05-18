using FluentAssertions;
using TechMove.GLMS.Core.Services.Strategies;
using Xunit;

namespace TechMove.GLMS.Tests.Strategies;

public class UsdToZarStrategyTests
{
    private readonly UsdToZarStrategy _strategy = new();

    [Fact]
    public void SourceCurrencyCode_Returns_USD()
    {
        _strategy.SourceCurrencyCode.Should().Be("USD");
    }

    [Fact]
    public void ConvertToZAR_HappyPath_ReturnsCorrectAmount()
    {
        // 100 USD at rate 18.50 = 1850.00 ZAR
        var result = _strategy.ConvertToZAR(amount: 100m, exchangeRate: 18.50m);
        result.Should().Be(1850.00m);
    }

    [Fact]
    public void ConvertToZAR_RoundsToTwoDecimalPlaces_AwayFromZero()
    {
        // 100 USD at rate 18.625 = 1862.50 (5 rounds up away from zero)
        var result = _strategy.ConvertToZAR(amount: 100m, exchangeRate: 18.625m);
        result.Should().Be(1862.50m);
    }

    [Fact]
    public void ConvertToZAR_ZeroAmount_ReturnsZero()
    {
        var result = _strategy.ConvertToZAR(amount: 0m, exchangeRate: 18.50m);
        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertToZAR_NegativeAmount_ThrowsArgumentException()
    {
        Action act = () => _strategy.ConvertToZAR(amount: -1m, exchangeRate: 18.50m);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*negative*");
    }

    [Fact]
    public void ConvertToZAR_ZeroExchangeRate_ThrowsArgumentException()
    {
        Action act = () => _strategy.ConvertToZAR(amount: 100m, exchangeRate: 0m);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*positive*");
    }

    [Fact]
    public void ConvertToZAR_NegativeExchangeRate_ThrowsArgumentException()
    {
        Action act = () => _strategy.ConvertToZAR(amount: 100m, exchangeRate: -5m);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(50, 18.624321, 931.22)]
    [InlineData(1, 18.50, 18.50)]
    [InlineData(0.01, 18.50, 0.19)]
    [InlineData(99999.99, 18.50, 1849999.82)]
    public void ConvertToZAR_VariousInputs_ProducesPreciseResults(
        double amount, double rate, double expected)
    {
        // Theory-driven: one method, four data rows, four named tests
        var result = _strategy.ConvertToZAR((decimal)amount, (decimal)rate);
        result.Should().Be((decimal)expected);
    }
}