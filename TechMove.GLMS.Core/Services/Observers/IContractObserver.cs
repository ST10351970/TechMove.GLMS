using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services.Observers;

/// Observer interface — implemented by anything that wants to be notified when a Contract's status changes.

public interface IContractObserver
{
    void OnContractStatusChanged(Contract contract, ContractStatus previousStatus, ContractStatus newStatus);
}