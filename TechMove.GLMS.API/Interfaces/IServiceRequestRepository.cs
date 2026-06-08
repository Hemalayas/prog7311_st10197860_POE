using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Interfaces;

public interface IServiceRequestRepository
{
    Task<IEnumerable<ServiceRequest>> GetAllAsync();
    Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId);
    Task<ServiceRequest?> GetByIdAsync(int id);
    Task AddAsync(ServiceRequest serviceRequest);
    Task DeleteAsync(int id);
}
