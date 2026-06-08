using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Services;

public class EmailNotificationObserver : IContractObserver
{
    public void Update(Contract contract, string oldStatus)
    {
        var client = contract.Client?.Name ?? $"ClientId:{contract.ClientId}";
        Console.WriteLine($"Email sent to {client} - contract status changed to {contract.Status}");
    }
}
