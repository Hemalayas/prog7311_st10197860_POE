using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Interfaces;

public interface IContractRepository
{
    Task<IEnumerable<Contract>> GetAllAsync();
    Task<Contract?> GetByIdAsync(int id);
    Task<IEnumerable<Contract>> SearchAsync(DateTime? startDate, DateTime? endDate, string? status);
    Task AddAsync(Contract contract);
    Task UpdateAsync(Contract contract);
    Task DeleteAsync(int id);
}
