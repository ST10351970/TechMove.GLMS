using FluentAssertions;
using TechMove.GLMS.Core.Enums;
using Xunit;

namespace TechMove.GLMS.Tests.StateMachine;

public class ContractStateTransitionsTests
{
    [Theory]
    [InlineData(ContractStatus.Draft, ContractStatus.Active)]
    [InlineData(ContractStatus.Draft, ContractStatus.Expired)]
    [InlineData(ContractStatus.Active, ContractStatus.Expired)]
    [InlineData(ContractStatus.Active, ContractStatus.OnHold)]
    [InlineData(ContractStatus.OnHold, ContractStatus.Active)]
    [InlineData(ContractStatus.OnHold, ContractStatus.Expired)]
    public void IsTransitionAllowed_LegalTransitions_ReturnTrue(
        ContractStatus from, ContractStatus to)
    {
        ContractStateTransitions.IsTransitionAllowed(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(ContractStatus.Expired, ContractStatus.Active)]   // terminal state
    [InlineData(ContractStatus.Expired, ContractStatus.Draft)]    // terminal state
    [InlineData(ContractStatus.Expired, ContractStatus.OnHold)]   // terminal state
    [InlineData(ContractStatus.Draft, ContractStatus.OnHold)]     // can't hold a draft
    [InlineData(ContractStatus.Active, ContractStatus.Draft)]     // can't undo activation
    [InlineData(ContractStatus.OnHold, ContractStatus.Draft)]     // can't reset to draft
    public void IsTransitionAllowed_IllegalTransitions_ReturnFalse(
        ContractStatus from, ContractStatus to)
    {
        ContractStateTransitions.IsTransitionAllowed(from, to).Should().BeFalse();
    }

    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Active)]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void IsTransitionAllowed_SameToSame_ReturnFalse(ContractStatus status)
    {
        ContractStateTransitions.IsTransitionAllowed(status, status).Should().BeFalse();
    }

    [Fact]
    public void GetAllowedTransitions_FromDraft_ReturnsActiveAndExpired()
    {
        var allowed = ContractStateTransitions.GetAllowedTransitions(ContractStatus.Draft);
        allowed.Should().BeEquivalentTo(new[] { ContractStatus.Active, ContractStatus.Expired });
    }

    [Fact]
    public void GetAllowedTransitions_FromExpired_ReturnsEmpty()
    {
        // Expired is terminal
        var allowed = ContractStateTransitions.GetAllowedTransitions(ContractStatus.Expired);
        allowed.Should().BeEmpty();
    }
}