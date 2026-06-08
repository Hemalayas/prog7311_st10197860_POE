using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.GLMS.Models;
using TechMove.GLMS.Services;
using TechMove.GLMS.ViewModels;

namespace TechMove.GLMS.Controllers;

public class ContractsController : Controller
{
    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;
    private readonly FileValidationService _fileValidation;

    public ContractsController(ApiService api, IWebHostEnvironment env, FileValidationService fileValidation)
    {
        _api = api;
        _env = env;
        _fileValidation = fileValidation;
    }

    public async Task<IActionResult> Index()
    {
        var contracts = await _api.GetContractsAsync();
        return View(contracts);
    }

    [HttpGet]
    public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string? status)
    {
        var vm = new ContractSearchViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            Searched = Request.Query.Count > 0,
        };

        if (vm.Searched)
            vm.Results = await _api.GetContractsAsync(status, startDate, endDate);

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _api.GetContractAsync(id);
        if (contract is null)
            return NotFound();

        return View(contract);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var clients = await _api.GetClientsAsync();
        ViewBag.ClientList = new SelectList(clients, "Id", "Name");
        return View(new Contract
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? SignedAgreement)
    {
        ModelState.Remove("Client");

        if (contract.ClientId == 0)
            ModelState.AddModelError(nameof(contract.ClientId), "Please select a client.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = string.Join(" | ", errors);
            var clients = await _api.GetClientsAsync();
            ViewBag.ClientList = new SelectList(clients, "Id", "Name");
            return View(contract);
        }

        if (SignedAgreement is not null)
        {
            if (!IsValidUpload(SignedAgreement, nameof(SignedAgreement)))
            {
                var clients = await _api.GetClientsAsync();
                ViewBag.ClientList = new SelectList(clients, "Id", "Name");
                return View(contract);
            }

            contract.SignedAgreementPath = await SavePdfAsync(SignedAgreement);
        }

        await _api.CreateContractAsync(contract);
        TempData["Success"] = "Contract created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _api.GetContractAsync(id);
        if (contract is null)
            return NotFound();

        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? SignedAgreement)
    {
        if (id != contract.Id)
            return BadRequest();

        if (SignedAgreement is not null)
        {
            if (!IsValidUpload(SignedAgreement, nameof(SignedAgreement)))
                return View(contract);

            contract.SignedAgreementPath = await SavePdfAsync(SignedAgreement);
        }

        if (!ModelState.IsValid)
            return View(contract);

        await _api.UpdateContractAsync(id, contract);
        TempData["Success"] = "Contract updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _api.GetContractAsync(id);
        if (contract is null)
            return NotFound();

        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteContractAsync(id);
        TempData["Success"] = "Contract deleted.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsValidUpload(IFormFile file, string fieldName)
    {
        if (_fileValidation.IsValidFile(file.FileName, file.Length))
            return true;

        ModelState.AddModelError(fieldName, "Only PDF files (.pdf) up to 10 MB are allowed.");
        return false;
    }

    private async Task<string> SavePdfAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "contracts");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}.pdf";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/contracts/{fileName}";
    }
}
