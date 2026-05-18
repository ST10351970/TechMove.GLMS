using System.ComponentModel.DataAnnotations;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services;

/// Validation service for Contract-related business rules.

public interface IContractValidator
{
    ValidationResult ValidateDates(DateTime startDate, DateTime endDate);

    ValidationResult ValidateServiceLevel(string? serviceLevel);

    ValidationResult ValidateStatusTransition(ContractStatus currentStatus, ContractStatus newStatus);

    /// Validates if new ServiceRequests are allowed against the given contract.

    ValidationResult ValidateCanAcceptServiceRequests(Contract contract);
}