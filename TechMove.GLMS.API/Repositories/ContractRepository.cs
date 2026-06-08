using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IEnumerable<IContractObserver> _observers;

    public ContractRepository(ApplicationDbContext context, IEnumerable<IContractObserver> observers)
    {
        _context = context;
        _observers = observers;
    }

    private void AttachObservers(Contract contract)
    {
        foreach (var observer in _observers)
            contract.Attach(observer);
    }

    public async Task<IEnumerable<Contract>> GetAllAsync()
    {
        var contracts = await _context.Contracts.Include(c => c.Client).ToListAsync();
        foreach (var contract in contracts)
            AttachObservers(contract);
        return contracts;
    }

    public async Task<Contract?> GetByIdAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is not null)
            AttachObservers(contract);

        return contract;
    }

    public async Task<IEnumerable<Contract>> SearchAsync(DateTime? startDate, DateTime? endDate, string? status)
    {
        var query = _context.Contracts.Include(c => c.Client).AsQueryable();

        if (startDate.HasValue)
            query = query.Where(c => c.StartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.EndDate <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContractStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(c => c.Status == parsedStatus);

        return await query.ToListAsync();
    }

    public async Task AddAsync(Contract contract)
    {
        await _context.Contracts.AddAsync(contract);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Contract contract)
    {
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract is not null)
        {
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }
    }
}
