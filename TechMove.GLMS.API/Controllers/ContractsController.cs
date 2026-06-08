using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractRepository _repo;

    public ContractsController(IContractRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate) =>
        Ok(await _repo.SearchAsync(startDate, endDate, status));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var contract = await _repo.GetByIdAsync(id);
        return contract is null
            ? NotFound(new { message = $"Contract with id {id} was not found." })
            : Ok(contract);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Contract contract)
    {
        await _repo.AddAsync(contract);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Contract contract)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Contract with id {id} was not found." });

        contract.Id = id;
        await _repo.UpdateAsync(contract);
        return Ok(contract);
    }

    [Authorize]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (!Enum.TryParse<ContractStatus>(request.Status, ignoreCase: true, out var newStatus))
            return BadRequest(new { message = $"Invalid status '{request.Status}'. Valid values: Draft, Active, Expired, OnHold." });

        var contract = await _repo.GetByIdAsync(id);
        if (contract is null)
            return NotFound(new { message = $"Contract with id {id} was not found." });

        contract.SetStatus(newStatus);
        await _repo.UpdateAsync(contract);
        return Ok(contract);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Contract with id {id} was not found." });

        await _repo.DeleteAsync(id);
        return NoContent();
    }
}

public record UpdateStatusRequest(string Status);
