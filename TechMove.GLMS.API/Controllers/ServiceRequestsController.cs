using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Controllers;

[ApiController]
[Route("api/servicerequests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestRepository _repo;
    private readonly IContractRepository _contractRepo;
    private readonly IWorkflowService _workflow;

    public ServiceRequestsController(
        IServiceRequestRepository repo,
        IContractRepository contractRepo,
        IWorkflowService workflow)
    {
        _repo = repo;
        _contractRepo = contractRepo;
        _workflow = workflow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? contractId)
    {
        var results = contractId.HasValue
            ? await _repo.GetByContractIdAsync(contractId.Value)
            : await _repo.GetAllAsync();
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _repo.GetByIdAsync(id);
        return request is null
            ? NotFound(new { message = $"Service request with id {id} was not found." })
            : Ok(request);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceRequest request)
    {
        var contract = await _contractRepo.GetByIdAsync(request.ContractId);
        if (contract is null)
            return NotFound(new { message = $"Contract with id {request.ContractId} was not found." });

        if (!_workflow.CanCreateServiceRequest(contract.Status))
            return BadRequest(new { message = $"Cannot create a service request for a contract with status '{contract.Status}'. Contract must be Active or Draft." });

        await _repo.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Service request with id {id} was not found." });

        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
