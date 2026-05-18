using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Services.Factories;

public class ContractFactory : IContractFactory
{
    // Centralised list of valid service levels — single source of truth
    public static readonly string[] ValidServiceLevels = { "Basic", "Premium", "Enterprise" };

    public Contract CreateContract(int clientId, string serviceLevel, DateTime? startDate = null)
    {
        if (clientId <= 0)
            throw new ArgumentException("ClientId must be positive.", nameof(clientId));

        if (string.IsNullOrWhiteSpace(serviceLevel))
            throw new ArgumentException("Service level is required.", nameof(serviceLevel));

        // Normalise casing so "basic", "Basic", "BASIC" all match
        var normalisedLevel = ValidServiceLevels
            .FirstOrDefault(s => string.Equals(s, serviceLevel, StringComparison.OrdinalIgnoreCase));

        if (normalisedLevel is null)
            throw new ArgumentException(
                $"Invalid service level '{serviceLevel}'. Allowed: {string.Join(", ", ValidServiceLevels)}.",
                nameof(serviceLevel));

        var start = startDate ?? DateTime.UtcNow.Date;
        var durationMonths = GetDefaultDurationMonths(normalisedLevel);

        return new Contract
        {
            ClientId = clientId,
            ServiceLevel = normalisedLevel,
            StartDate = start,
            EndDate = start.AddMonths(durationMonths),
            Status = ContractStatus.Draft
        };
    }

    /// Service-level-specific defaults.
    private static int GetDefaultDurationMonths(string serviceLevel) => serviceLevel switch
    {
        "Basic" => 6,
        "Premium" => 12,
        "Enterprise" => 24,
        _ => throw new InvalidOperationException(
            $"Unhandled service level '{serviceLevel}'. This indicates a bug in ValidServiceLevels.")
    };
}