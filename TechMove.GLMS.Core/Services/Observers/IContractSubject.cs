namespace TechMove.GLMS.Core.Services.Observers;

/// Subject interface — implemented by anything that broadcasts contract status changes to registered observers.

public interface IContractSubject
{
    void Attach(IContractObserver observer);
    void Detach(IContractObserver observer);
    void NotifyStatusChanged(Entities.Contract contract,
                             Enums.ContractStatus previousStatus,
                             Enums.ContractStatus newStatus);
}