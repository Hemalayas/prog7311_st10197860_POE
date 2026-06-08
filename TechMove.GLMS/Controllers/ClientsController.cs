using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.Models;
using TechMove.GLMS.Services;

namespace TechMove.GLMS.Controllers;

public class ClientsController : Controller
{
    private readonly ApiService _api;

    public ClientsController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var clients = await _api.GetClientsAsync();
        return View(clients);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = await _api.GetClientAsync(id);
        if (client is null)
            return NotFound();

        return View(client);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Client());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid)
            return View(client);

        await _api.CreateClientAsync(client);
        TempData["Success"] = "Client created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _api.GetClientAsync(id);
        if (client is null)
            return NotFound();

        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(client);

        await _api.UpdateClientAsync(id, client);
        TempData["Success"] = "Client updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _api.GetClientAsync(id);
        if (client is null)
            return NotFound();

        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteClientAsync(id);
        TempData["Success"] = "Client deleted.";
        return RedirectToAction(nameof(Index));
    }
}
