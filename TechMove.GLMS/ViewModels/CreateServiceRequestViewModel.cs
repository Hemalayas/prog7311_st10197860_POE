using System.ComponentModel.DataAnnotations;

namespace TechMove.GLMS.ViewModels;

public class CreateServiceRequestViewModel
{
    // No [Remote] — server-side workflow check runs in the POST action
    [Required]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Please select a service type.")]
    [Display(Name = "Service Type")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cost is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
    [Display(Name = "Cost (USD)")]
    public decimal? Cost { get; set; }

    [Display(Name = "Cost (ZAR)")]
    public decimal CostZAR { get; set; }
}
