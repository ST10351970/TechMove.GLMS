namespace TechMove.GLMS.Core.Enums;

// Legal Contract status transitions

public static class ContractStateTransitions
{
    //Allowed Transitions:
    //   Draft   -> Active | Expired (cancelled before activation)
    //   Active  -> Expired | OnHold
    //   OnHold  -> Active  | Expired
    //   Expired -> (terminal — no transitions allowed)
    private static readonly Dictionary<ContractStatus, HashSet<ContractStatus>> Allowed = new()
    {
        [ContractStatus.Draft] = new() { ContractStatus.Active, ContractStatus.Expired },
        [ContractStatus.Active] = new() { ContractStatus.Expired, ContractStatus.OnHold },
        [ContractStatus.OnHold] = new() { ContractStatus.Active, ContractStatus.Expired },
        [ContractStatus.Expired] = new() // terminal state
    };

    public static bool IsTransitionAllowed(ContractStatus from, ContractStatus to)
    {
        if (from == to) return false; 
        return Allowed.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
    }

    public static IReadOnlyCollection<ContractStatus> GetAllowedTransitions(ContractStatus from)
    {
        return Allowed.TryGetValue(from, out var validTargets)
            ? validTargets.ToList().AsReadOnly()
            : new List<ContractStatus>().AsReadOnly();
    }
}