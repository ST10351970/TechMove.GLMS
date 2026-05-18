using TechMove.GLMS.Core.Entities;

namespace TechMove.GLMS.Core.Services.Factories;

/// Factory for creating Contract entities with per-service-level defaults applied.

public interface IContractFactory
{
    /// Creates a new Contract for the given Client at the requested service level. Appropriate defaults (duration, status, etc.)

    Contract CreateContract(int clientId, string serviceLevel, DateTime? startDate = null);
}