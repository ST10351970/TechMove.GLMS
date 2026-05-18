using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services.Factories;

namespace TechMove.GLMS.Core.Services;

public class ContractValidator : IContractValidator
{
    public ValidationResult ValidateDates(DateTime startDate, DateTime endDate)
    {
        var errors = new List<string>();

        if (startDate == default)
            errors.Add("Start date is required.");

        if (endDate == default)
            errors.Add("End date is required.");

        if (startDate != default && endDate != default && endDate <= startDate)
            errors.Add("End date must be after the start date.");

        // Edge case for contract starting more than a year in the past
        if (startDate != default && startDate < DateTime.UtcNow.Date.AddYears(-1))
            errors.Add("Start date cannot be more than one year in the past.");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    public ValidationResult ValidateServiceLevel(string? serviceLevel)
    {
        if (string.IsNullOrWhiteSpace(serviceLevel))
            return ValidationResult.Failure("Service level is required.");

        var isValid = ContractFactory.ValidServiceLevels
            .Any(s => string.Equals(s, serviceLevel, StringComparison.OrdinalIgnoreCase));

        if (!isValid)
            return ValidationResult.Failure(
                $"Service level must be one of: {string.Join(", ", ContractFactory.ValidServiceLevels)}.");

        return ValidationResult.Success();
    }

    public ValidationResult ValidateStatusTransition(ContractStatus currentStatus, ContractStatus newStatus)
    {
        if (currentStatus == newStatus)
            return ValidationResult.Failure(
                $"Contract is already in status '{currentStatus}'.");

        if (!ContractStateTransitions.IsTransitionAllowed(currentStatus, newStatus))
        {
            var allowed = ContractStateTransitions.GetAllowedTransitions(currentStatus);
            var allowedSummary = allowed.Count == 0
                ? "no further transitions are allowed"
                : $"allowed transitions are: {string.Join(", ", allowed)}";

            return ValidationResult.Failure(
                $"Cannot transition from '{currentStatus}' to '{newStatus}'. From '{currentStatus}', {allowedSummary}.");
        }

        return ValidationResult.Success();
    }

    public ValidationResult ValidateCanAcceptServiceRequests(Contract contract)
    {
        if (contract is null)
            return ValidationResult.Failure("Contract not found.");

        return contract.Status switch
        {
            ContractStatus.Expired => ValidationResult.Failure(
                $"Cannot create service request: contract #{contract.Id} is Expired."),
            ContractStatus.OnHold => ValidationResult.Failure(
                $"Cannot create service request: contract #{contract.Id} is currently On Hold."),
            ContractStatus.Draft => ValidationResult.Failure(
                $"Cannot create service request: contract #{contract.Id} is still in Draft and has not been activated."),
            ContractStatus.Active => ValidationResult.Success(),
            _ => ValidationResult.Failure($"Unknown contract status '{contract.Status}'.")
        };
    }
}