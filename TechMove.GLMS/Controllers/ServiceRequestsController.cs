using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.Models;
using TechMove.GLMS.Services;
using TechMove.GLMS.ViewModels;

namespace TechMove.GLMS.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApiService _api;
    private readonly CurrencyService _currencyService;
    private readonly WorkflowValidationService _workflowValidation;

    public ServiceRequestsController(
        ApiService api,
        CurrencyService currencyService,
        WorkflowValidationService workflowValidation)
    {
        _api = api;
        _currencyService = currencyService;
        _workflowValidation = workflowValidation;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _api.GetServiceRequestsAsync();
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> GetZarRate()
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();
        return Json(new { rate });
    }

    // Called by [Remote] attribute via AJAX — returns true or an error message
    [HttpGet]
    public async Task<IActionResult> ValidateContract(int contractId)
    {
        var contract = await _api.GetContractAsync(contractId);
        if (contract is null)
            return Json("Contract not found.");

        if (!_workflowValidation.CanCreateServiceRequest(contract.Status))
            return Json("Cannot create service request for an expired or on-hold contract.");

        return Json(true);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int contractId)
    {
        var contract = await _api.GetContractAsync(contractId);
        if (contract is null)
            return NotFound();

        ViewBag.Contract = contract;
        return View(new CreateServiceRequestViewModel { ContractId = contractId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceRequestViewModel vm)
    {
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            Console.WriteLine($"[ModelState Error] {error.ErrorMessage}");

        // The ZAR value is formatted with a dot by JS (toFixed(2)) but the model binder
        // may reject it if the server culture uses commas — parse it manually instead.
        ModelState.Remove(nameof(vm.CostZAR));
        if (decimal.TryParse(Request.Form["CostZAR"],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal costZar))
        {
            vm.CostZAR = costZar;
        }

        var contract = await _api.GetContractAsync(vm.ContractId);

        if (contract is null)
        {
            ModelState.AddModelError(nameof(vm.ContractId), "Contract not found.");
        }
        else if (!_workflowValidation.CanCreateServiceRequest(contract.Status))
        {
            ModelState.AddModelError(nameof(vm.ContractId),
                "Cannot create service request for an expired or on-hold contract.");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            ViewBag.Contract = contract;
            return View(vm);
        }

        var serviceRequest = ServiceRequestFactory.Create(vm.Type, vm.ContractId, vm.Description, vm.Cost!.Value);
        serviceRequest.CostZAR = vm.CostZAR;
        await _api.CreateServiceRequestAsync(serviceRequest);

        TempData["Success"] = "Service request created successfully.";
        return RedirectToAction("Details", "Contracts", new { id = vm.ContractId });
    }
}
