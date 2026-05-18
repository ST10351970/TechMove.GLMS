using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services.Observers;

/// Observer writes an entry to application's audit trail when a contract's status change.

public class AuditLogObserver : IContractObserver
{
    //in-memory list for unit tests
    private readonly List<string> _entries = new();
    public IReadOnlyList<string> Entries => _entries.AsReadOnly();

    public void OnContractStatusChanged(Contract contract,
                                        ContractStatus previousStatus,
                                        ContractStatus newStatus)
    {
        var entry = $"[{DateTime.UtcNow:O}] Contract #{contract.Id} status changed: {previousStatus} -> {newStatus}";
        _entries.Add(entry);
    }
}