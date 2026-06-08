using TechMove.GLMS.Models;

namespace TechMove.GLMS.Services;

public static class ServiceRequestFactory
{
    public static ServiceRequest Create(string type, int contractId, string description, decimal cost)
    {
        var serviceLevel = type switch
        {
            "Freight"   => "Standard Freight Handling — door-to-door land/air/sea transport",
            "Customs"   => "Customs Clearance Processing — import/export documentation and duties",
            "Warehouse" => "Warehouse Storage & Handling — receiving, storage, and dispatch",
            _           => throw new ArgumentException($"Unknown service request type: '{type}'", nameof(type))
        };

        return new ServiceRequest
        {
            ContractId   = contractId,
            Description  = description,
            ServiceLevel = serviceLevel,
            Cost         = cost,
            Status       = ServiceRequestStatus.Pending
        };
    }
}
