using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories;

public class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        => await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .OrderByDescending(sr => sr.Id)
            .ToListAsync();

    public async Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId)
        => await _context.ServiceRequests
            .Where(sr => sr.ContractId == contractId)
            .ToListAsync();

    public async Task<ServiceRequest?> GetByIdAsync(int id)
        => await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .FirstOrDefaultAsync(sr => sr.Id == id);

    public async Task AddAsync(ServiceRequest serviceRequest)
    {
        await _context.ServiceRequests.AddAsync(serviceRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request is not null)
        {
            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}
