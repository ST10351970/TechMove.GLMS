using FluentAssertions;
using Moq;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services.Observers;
using Xunit;

namespace TechMove.GLMS.Tests.Observers;

public class ContractStatusNotifierTests
{
    [Fact]
    public void NotifyStatusChanged_BroadcastsToAllAttachedObservers()
    {
        // two mocked observers
        var mockObserver1 = new Mock<IContractObserver>();
        var mockObserver2 = new Mock<IContractObserver>();
        var notifier = new ContractStatusNotifier(
            new[] { mockObserver1.Object, mockObserver2.Object });

        var contract = new Contract { Id = 42, Status = ContractStatus.Active };

        // Act
        notifier.NotifyStatusChanged(contract, ContractStatus.Draft, ContractStatus.Active);

        // Assert: both observers called exactly once with the right args
        mockObserver1.Verify(o => o.OnContractStatusChanged(
            contract, ContractStatus.Draft, ContractStatus.Active), Times.Once);
        mockObserver2.Verify(o => o.OnContractStatusChanged(
            contract, ContractStatus.Draft, ContractStatus.Active), Times.Once);
    }

    [Fact]
    public void NotifyStatusChanged_NoOpIfPreviousAndNewStatusAreSame()
    {
        var mockObserver = new Mock<IContractObserver>();
        var notifier = new ContractStatusNotifier(new[] { mockObserver.Object });
        var contract = new Contract { Id = 1, Status = ContractStatus.Active };

        notifier.NotifyStatusChanged(contract, ContractStatus.Active, ContractStatus.Active);

        // Observer should NOT be called when there's no actual change
        mockObserver.Verify(o => o.OnContractStatusChanged(
            It.IsAny<Contract>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()),
            Times.Never);
    }

    [Fact]
    public void NotifyStatusChanged_OneObserverThrowing_DoesNotPreventOthers()
    {
        // Resilience test: if observer A explodes, observer B should still be called
        var mockObserverA = new Mock<IContractObserver>();
        mockObserverA.Setup(o => o.OnContractStatusChanged(
            It.IsAny<Contract>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
            .Throws(new InvalidOperationException("simulated failure"));

        var mockObserverB = new Mock<IContractObserver>();
        var notifier = new ContractStatusNotifier(
            new[] { mockObserverA.Object, mockObserverB.Object });

        var contract = new Contract { Id = 1 };

        // Should NOT throw — the notifier swallows individual observer failures
        Action act = () => notifier.NotifyStatusChanged(
            contract, ContractStatus.Draft, ContractStatus.Active);
        act.Should().NotThrow();

        // Observer B should still have been called even though A failed
        mockObserverB.Verify(o => o.OnContractStatusChanged(
            contract, ContractStatus.Draft, ContractStatus.Active), Times.Once);
    }

    [Fact]
    public void Detach_PreventsFurtherNotifications()
    {
        var mockObserver = new Mock<IContractObserver>();
        var notifier = new ContractStatusNotifier(new[] { mockObserver.Object });

        notifier.Detach(mockObserver.Object);

        notifier.NotifyStatusChanged(
            new Contract { Id = 1 }, ContractStatus.Draft, ContractStatus.Active);

        mockObserver.Verify(o => o.OnContractStatusChanged(
            It.IsAny<Contract>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()),
            Times.Never);
    }
}