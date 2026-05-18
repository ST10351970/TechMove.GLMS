using System.Collections.Concurrent;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services.Observers;

/// Observer tracks which contracts have transitioned to Expired or OnHold

public class ExpiredContractGuardObserver : IContractObserver
{
    private readonly ConcurrentDictionary<int, ContractStatus> _lockedContracts = new();

    public void OnContractStatusChanged(Contract contract,
                                        ContractStatus previousStatus,
                                        ContractStatus newStatus)
    {
        if (newStatus == ContractStatus.Expired || newStatus == ContractStatus.OnHold)
        {
            _lockedContracts[contract.Id] = newStatus;
        }
        else
        {
            _lockedContracts.TryRemove(contract.Id, out _);
        }
    }

    public bool IsLocked(int contractId) => _lockedContracts.ContainsKey(contractId);

    public ContractStatus? GetLockReason(int contractId)
        => _lockedContracts.TryGetValue(contractId, out var status) ? status : null;
}