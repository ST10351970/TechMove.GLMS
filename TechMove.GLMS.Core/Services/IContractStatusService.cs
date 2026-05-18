using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services;

public interface IContractStatusService
{
    /// <summary>
    /// Attempts to change a contract's status, validating the transition and notifying observers on success.
    /// </summary>
    Task<ValidationResult> ChangeStatusAsync(int contractId, ContractStatus newStatus, CancellationToken ct = default);
}