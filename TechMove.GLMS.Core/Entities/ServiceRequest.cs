using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.GLMS.Core.Entities;

public class ServiceRequest
{
    public int Id { get; set; }

    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Cost in source currency (e.g. USD entered by the user)
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cost (Source Currency)")]
    public decimal CostInSourceCurrency { get; set; }

    [Required]
    [StringLength(3)]
    [Display(Name = "Source Currency")]
    public string SourceCurrency { get; set; } = "USD";

    // Calculated ZAR cost
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cost (ZAR)")]
    public decimal CostInZAR { get; set; }

    // Exchange rate used at the time of calculation (audit trail)
    [Column(TypeName = "decimal(18,6)")]
    [Display(Name = "Exchange Rate Used")]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}