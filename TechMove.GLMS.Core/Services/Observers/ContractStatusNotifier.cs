using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services.Observers;

/// Concrete subject. Holds the list of observers and broadcasts status change events.

public class ContractStatusNotifier : IContractSubject
{
    private readonly List<IContractObserver> _observers = new();
    private readonly object _lock = new();

    public ContractStatusNotifier(IEnumerable<IContractObserver> observers)
    {
        // Auto-attach every observer that was registered in DI
        foreach (var observer in observers)
        {
            Attach(observer);
        }
    }

    public void Attach(IContractObserver observer)
    {
        if (observer is null) throw new ArgumentNullException(nameof(observer));
        lock (_lock)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
    }

    public void Detach(IContractObserver observer)
    {
        if (observer is null) throw new ArgumentNullException(nameof(observer));
        lock (_lock)
        {
            _observers.Remove(observer);
        }
    }

    public void NotifyStatusChanged(Contract contract,
                                    ContractStatus previousStatus,
                                    ContractStatus newStatus)
    {
        if (contract is null) throw new ArgumentNullException(nameof(contract));
        if (previousStatus == newStatus) return;

        List<IContractObserver> snapshot;
        lock (_lock)
        {
            snapshot = _observers.ToList();
        }

        foreach (var observer in snapshot)
        {
            try
            {
                observer.OnContractStatusChanged(contract, previousStatus, newStatus);
            }
            catch (Exception)
            {
                
            }
        }
    }
}