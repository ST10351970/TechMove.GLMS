using System.ComponentModel.DataAnnotations;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Entities;

public class Contract
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    [Required]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [StringLength(50)]
    [Display(Name = "Service Level")]
    public string ServiceLevel { get; set; } = string.Empty;

    // PDF signed agreement — stored as path on disk
    [StringLength(500)]
    public string? SignedAgreementPath { get; set; }

    [StringLength(255)]
    public string? SignedAgreementOriginalName { get; set; }

    // Navigation property — a contract can have many service requests
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}