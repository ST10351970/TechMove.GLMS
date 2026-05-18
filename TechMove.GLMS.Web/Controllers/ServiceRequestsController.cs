using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Services;
using TechMove.GLMS.Core.Services.CurrencyExchange;
using TechMove.GLMS.Core.Services.Strategies;

namespace TechMove.GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IContractValidator _validator;
    private readonly ICurrencyExchangeService _exchangeService;
    private readonly CurrencyStrategyResolver _strategyResolver;

    public ServiceRequestsController(
        ApplicationDbContext db,
        IContractValidator validator,
        ICurrencyExchangeService exchangeService,
        CurrencyStrategyResolver strategyResolver)
    {
        _db = db;
        _validator = validator;
        _exchangeService = exchangeService;
        _strategyResolver = strategyResolver;
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
        string sourceCurrency,
        CancellationToken ct)
    {
        var contract = await _db.Contracts.FindAsync(contractId);
        if (contract is null) return NotFound();

        // Contract-status check
        var canAccept = _validator.ValidateCanAcceptServiceRequests(contract);
        if (!canAccept.IsValid)
        {
            TempData["Error"] = canAccept.ErrorSummary;
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }

        // Input validation
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

        //Strategy pattern + external API integration
        var rateResult = await _exchangeService.GetRateToZarAsync(sourceCurrency, ct);
        if (!rateResult.Success)
        {
            ModelState.AddModelError(string.Empty,
                $"Could not retrieve exchange rate: {rateResult.ErrorMessage}");
            ViewBag.ContractId = contractId;
            return View();
        }

        var strategy = _strategyResolver.Resolve(sourceCurrency);
        var costInZar = strategy.ConvertToZAR(costInSourceCurrency, rateResult.Rate);

        var serviceRequest = new ServiceRequest
        {
            ContractId = contractId,
            Description = description,
            CostInSourceCurrency = costInSourceCurrency,
            SourceCurrency = sourceCurrency.ToUpperInvariant(),
            CostInZAR = costInZar,
            ExchangeRateUsed = rateResult.Rate,
            Status = "Pending"
        };

        _db.ServiceRequests.Add(serviceRequest);
        await _db.SaveChangesAsync(ct);

        var note = rateResult.FromCache ? " (using cached rate)" : "";
        TempData["Success"] =
            $"Service request #{serviceRequest.Id} created. {costInSourceCurrency} {sourceCurrency.ToUpperInvariant()} = R{costInZar:N2}{note}.";

        return RedirectToAction("Details", "Contracts", new { id = contractId });
    }

    // GET: /ServiceRequests/CalculateZar?amount=100&currency=USD
    // AJAX endpoint for live calculation
    [HttpGet]
    public async Task<IActionResult> CalculateZar(decimal amount, string currency, CancellationToken ct)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(currency))
            return Json(new { success = false, error = "Amount and currency are required." });

        var rateResult = await _exchangeService.GetRateToZarAsync(currency, ct);
        if (!rateResult.Success)
            return Json(new { success = false, error = rateResult.ErrorMessage });

        var strategy = _strategyResolver.Resolve(currency);
        var costInZar = strategy.ConvertToZAR(amount, rateResult.Rate);

        return Json(new
        {
            success = true,
            costInZar,
            rate = rateResult.Rate,
            fromCache = rateResult.FromCache,
            formatted = $"R{costInZar:N2}"
        });
    }
}