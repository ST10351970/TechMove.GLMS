using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services.Observers;

namespace TechMove.GLMS.Core.Services;

public class ContractStatusService : IContractStatusService
{
    private readonly ApplicationDbContext _db;
    private readonly IContractValidator _validator;
    private readonly IContractSubject _statusNotifier;

    public ContractStatusService(
        ApplicationDbContext db,
        IContractValidator validator,
        IContractSubject statusNotifier)
    {
        _db = db;
        _validator = validator;
        _statusNotifier = statusNotifier;
    }

    public async Task<ValidationResult> ChangeStatusAsync(
        int contractId,
        ContractStatus newStatus,
        CancellationToken ct = default)
    {
        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.Id == contractId, ct);
        if (contract is null)
            return ValidationResult.Failure($"Contract #{contractId} not found.");

        var transitionResult = _validator.ValidateStatusTransition(contract.Status, newStatus);
        if (!transitionResult.IsValid)
            return transitionResult;

        var previousStatus = contract.Status;
        contract.Status = newStatus;

        await _db.SaveChangesAsync(ct);

        // Observer broadcast — calls AuditLogObserver and ExpiredContractGuardObserver
        _statusNotifier.NotifyStatusChanged(contract, previousStatus, newStatus);

        return ValidationResult.Success();
    }
}