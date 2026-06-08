using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Interfaces;

public interface IWorkflowService
{
    bool CanCreateServiceRequest(ContractStatus status);
}
