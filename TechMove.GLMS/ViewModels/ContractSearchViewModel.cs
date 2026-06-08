using System.ComponentModel.DataAnnotations;
using TechMove.GLMS.Models;

namespace TechMove.GLMS.ViewModels;

public class ContractSearchViewModel
{
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Status")]
    public string? Status { get; set; }

    public IEnumerable<Contract> Results { get; set; } = Enumerable.Empty<Contract>();

    public bool Searched { get; set; }
}
