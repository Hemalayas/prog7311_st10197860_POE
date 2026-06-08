using System.ComponentModel.DataAnnotations;

namespace TechMove.GLMS.API.Models;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ContactDetails { get; set; }

    [StringLength(100)]
    public string? Region { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
