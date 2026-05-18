using FluentAssertions;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services.Factories;
using Xunit;

namespace TechMove.GLMS.Tests.Factories;

public class ContractFactoryTests
{
    private readonly ContractFactory _factory = new();

    [Fact]
    public void CreateContract_HappyPath_ReturnsContractWithDraftStatus()
    {
        var contract = _factory.CreateContract(
            clientId: 1, serviceLevel: "Premium", startDate: new DateTime(2026, 6, 1));

        contract.ClientId.Should().Be(1);
        contract.ServiceLevel.Should().Be("Premium");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Theory]
    [InlineData("Basic", 6)]
    [InlineData("Premium", 12)]
    [InlineData("Enterprise", 24)]
    public void CreateContract_AppliesCorrectDurationForServiceLevel(string level, int expectedMonths)
    {
        var start = new DateTime(2026, 1, 1);
        var contract = _factory.CreateContract(clientId: 1, serviceLevel: level, startDate: start);

        contract.EndDate.Should().Be(start.AddMonths(expectedMonths));
    }

    [Fact]
    public void CreateContract_IsCaseInsensitiveOnServiceLevel()
    {
        var contract = _factory.CreateContract(
            clientId: 1, serviceLevel: "premium", startDate: new DateTime(2026, 1, 1));

        // Factory normalises to title case
        contract.ServiceLevel.Should().Be("Premium");
    }

    [Fact]
    public void CreateContract_InvalidServiceLevel_ThrowsArgumentException()
    {
        Action act = () => _factory.CreateContract(
            clientId: 1, serviceLevel: "Diamond", startDate: new DateTime(2026, 1, 1));

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Diamond*");
    }

    [Fact]
    public void CreateContract_NullOrEmptyServiceLevel_ThrowsArgumentException()
    {
        Action actNull = () => _factory.CreateContract(1, null!, DateTime.UtcNow);
        Action actEmpty = () => _factory.CreateContract(1, "", DateTime.UtcNow);

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateContract_InvalidClientId_ThrowsArgumentException()
    {
        Action actZero = () => _factory.CreateContract(0, "Basic", DateTime.UtcNow);
        Action actNegative = () => _factory.CreateContract(-5, "Basic", DateTime.UtcNow);

        actZero.Should().Throw<ArgumentException>();
        actNegative.Should().Throw<ArgumentException>();
    }
}