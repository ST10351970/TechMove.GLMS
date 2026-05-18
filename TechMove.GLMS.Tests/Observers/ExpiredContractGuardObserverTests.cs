using FluentAssertions;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services.Observers;
using Xunit;

namespace TechMove.GLMS.Tests.Observers;

public class ExpiredContractGuardObserverTests
{
    [Fact]
    public void OnStatusChanged_ToExpired_LocksContract()
    {
        var guard = new ExpiredContractGuardObserver();
        var contract = new Contract { Id = 5 };

        guard.OnContractStatusChanged(contract, ContractStatus.Active, ContractStatus.Expired);

        guard.IsLocked(5).Should().BeTrue();
        guard.GetLockReason(5).Should().Be(ContractStatus.Expired);
    }

    [Fact]
    public void OnStatusChanged_ToOnHold_LocksContract()
    {
        var guard = new ExpiredContractGuardObserver();
        var contract = new Contract { Id = 7 };

        guard.OnContractStatusChanged(contract, ContractStatus.Active, ContractStatus.OnHold);

        guard.IsLocked(7).Should().BeTrue();
        guard.GetLockReason(7).Should().Be(ContractStatus.OnHold);
    }

    [Fact]
    public void OnStatusChanged_BackToActive_RemovesLock()
    {
        var guard = new ExpiredContractGuardObserver();
        var contract = new Contract { Id = 9 };

        guard.OnContractStatusChanged(contract, ContractStatus.Active, ContractStatus.OnHold);
        guard.IsLocked(9).Should().BeTrue();

        guard.OnContractStatusChanged(contract, ContractStatus.OnHold, ContractStatus.Active);
        guard.IsLocked(9).Should().BeFalse();
    }

    [Fact]
    public void IsLocked_UnknownContract_ReturnsFalse()
    {
        var guard = new ExpiredContractGuardObserver();
        guard.IsLocked(999).Should().BeFalse();
        guard.GetLockReason(999).Should().BeNull();
    }
}