using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.GLMS.Core.Entities;

public class ServiceRequest
{
    public int Id { get; set; }

    // Foreign key back to Contract
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Cost in source currency (e.g. USD entered by the user)
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostInSourceCurrency { get; set; }

    [Required]
    [StringLength(3)]
    public string SourceCurrency { get; set; } = "USD";

    // Calculated ZAR cost — what TechMove reports against
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostInZAR { get; set; }

    // Exchange rate used at the time of calculation (audit trail)
    [Column(TypeName = "decimal(18,6)")]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}