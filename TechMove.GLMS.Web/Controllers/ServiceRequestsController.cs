using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Services;

namespace TechMove.GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IContractValidator _validator;

    public ServiceRequestsController(ApplicationDbContext db, IContractValidator validator)
    {
        _db = db;
        _validator = validator;
    }

    // GET: /ServiceRequests/Create?contractId=5
    public async Task<IActionResult> Create(int contractId)
    {
        var contract = await _db.Contracts.FindAsync(contractId);
        if (contract is null) return NotFound();

        var canAccept = _validator.ValidateCanAcceptServiceRequests(contract);
        if (!canAccept.IsValid)
        {
            TempData["Error"] = canAccept.ErrorSummary;
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }

        ViewBag.ContractId = contractId;
        return View();
    }

    // POST: /ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int contractId,
        string description,
        decimal costInSourceCurrency,
        string sourceCurrency)
    {
        var contract = await _db.Contracts.FindAsync(contractId);
        if (contract is null) return NotFound();

        var canAccept = _validator.ValidateCanAcceptServiceRequests(contract);
        if (!canAccept.IsValid)
        {
            TempData["Error"] = canAccept.ErrorSummary;
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }

        if (string.IsNullOrWhiteSpace(description))
            ModelState.AddModelError(nameof(description), "Description is required.");
        if (costInSourceCurrency <= 0)
            ModelState.AddModelError(nameof(costInSourceCurrency), "Cost must be greater than zero.");
        if (string.IsNullOrWhiteSpace(sourceCurrency))
            ModelState.AddModelError(nameof(sourceCurrency), "Source currency is required.");

        if (!ModelState.IsValid)
        {
            ViewBag.ContractId = contractId;
            return View();
        }

        var serviceRequest = new ServiceRequest
        {
            ContractId = contractId,
            Description = description,
            CostInSourceCurrency = costInSourceCurrency,
            SourceCurrency = sourceCurrency.ToUpperInvariant(),
            CostInZAR = 0m,
            ExchangeRateUsed = 0m,
            Status = "Pending"
        };

        _db.ServiceRequests.Add(serviceRequest);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Service request #{serviceRequest.Id} created.";
        return RedirectToAction("Details", "Contracts", new { id = contractId });
    }
}