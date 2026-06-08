using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Services;

public class AuditLogObserver : IContractObserver
{
    public List<string> AuditTrail { get; } = new();

    public void Update(Contract contract, string oldStatus)
    {
        AuditTrail.Add(
            $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Contract {contract.Id} status changed from {oldStatus} to {contract.Status}");
    }
}
