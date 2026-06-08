using TechMove.GLMS.Models;

namespace TechMove.GLMS.Services;

public class WorkflowValidationService
{
    public bool CanCreateServiceRequest(ContractStatus status)
        => status is not (ContractStatus.Expired or ContractStatus.OnHold);
}
