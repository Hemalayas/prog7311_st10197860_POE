using TechMove.GLMS.Interfaces;
using TechMove.GLMS.Models;

namespace TechMove.GLMS.Services;

public class EmailNotificationObserver : IContractObserver
{
    public void Update(Contract contract, string oldStatus)
    {
        var client = contract.Client?.Name ?? $"ClientId:{contract.ClientId}";
        Console.WriteLine($"Email sent to {client} - contract status changed to {contract.Status}");
    }
}
