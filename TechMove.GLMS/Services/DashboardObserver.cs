using TechMove.GLMS.Interfaces;
using TechMove.GLMS.Models;

namespace TechMove.GLMS.Services;

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
