using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Services;

public class WorkflowService : IWorkflowService
{
    public bool CanCreateServiceRequest(ContractStatus status) =>
        status is not (ContractStatus.Expired or ContractStatus.OnHold);
}
