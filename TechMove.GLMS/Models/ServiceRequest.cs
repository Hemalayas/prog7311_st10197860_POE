using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechMove.GLMS.Interfaces;

namespace TechMove.GLMS.Models;

public enum ServiceRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class ServiceRequest : IServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string ServiceLevel { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostZAR { get; set; }

    [Required]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

    [ForeignKey(nameof(ContractId))]
    public Contract Contract { get; set; } = null!;
}
