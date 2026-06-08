using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Services;

public class DashboardObserver : IContractObserver
{
    public static int ActiveContractCount { get; private set; }

    public void Update(Contract contract, string oldStatus)
    {
        if (contract.Status == ContractStatus.Active)
            ActiveContractCount++;
        else if (oldStatus == nameof(ContractStatus.Active))
            ActiveContractCount--;
    }
}
