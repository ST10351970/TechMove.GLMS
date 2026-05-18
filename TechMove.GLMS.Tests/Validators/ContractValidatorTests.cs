using FluentAssertions;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services;
using Xunit;

namespace TechMove.GLMS.Tests.Validators;

public class ContractValidatorTests
{
    private readonly ContractValidator _validator = new();

    [Fact]
    public void ValidateDates_EndBeforeStart_ReturnsFailure()
    {
        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 5, 1);

        var result = _validator.ValidateDates(start, end);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("after");
    }

    [Fact]
    public void ValidateDates_ValidRange_ReturnsSuccess()
    {
        var result = _validator.ValidateDates(
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddMonths(6));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    [InlineData(ContractStatus.Draft)]
    public void ValidateCanAcceptServiceRequests_NonActiveContract_ReturnsFailure(
        ContractStatus status)
    {
        var contract = new Contract { Id = 1, Status = status };

        var result = _validator.ValidateCanAcceptServiceRequests(contract);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("#1");
    }

    [Fact]
    public void ValidateCanAcceptServiceRequests_ActiveContract_ReturnsSuccess()
    {
        var contract = new Contract { Id = 1, Status = ContractStatus.Active };

        var result = _validator.ValidateCanAcceptServiceRequests(contract);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCanAcceptServiceRequests_NullContract_ReturnsFailure()
    {
        var result = _validator.ValidateCanAcceptServiceRequests(null!);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("not found");
    }
}