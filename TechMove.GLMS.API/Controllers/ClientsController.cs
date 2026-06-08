using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _repo;

    public ClientsController(IClientRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _repo.GetByIdAsync(id);
        return client is null
            ? NotFound(new { message = $"Client with id {id} was not found." })
            : Ok(client);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client client)
    {
        await _repo.AddAsync(client);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Client client)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Client with id {id} was not found." });

        client.Id = id;
        await _repo.UpdateAsync(client);
        return Ok(client);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Client with id {id} was not found." });

        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
