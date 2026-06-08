using TechMove.GLMS.Models;

namespace TechMove.GLMS.Interfaces;

public interface IServiceRequest
{
    int Id { get; set; }
    int ContractId { get; set; }
    string Description { get; set; }
    string ServiceLevel { get; set; }
    decimal Cost { get; set; }
    decimal CostZAR { get; set; }
    ServiceRequestStatus Status { get; set; }
}
