using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;
using TechMove.GLMS.Core.Services;
using TechMove.GLMS.Core.Services.Factories;

namespace TechMove.GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IContractFactory _contractFactory;
    private readonly IContractValidator _validator;
    private readonly IContractStatusService _statusService;
    private readonly IFileService _fileService;

    public ContractsController(
        ApplicationDbContext db,
        IContractFactory contractFactory,
        IContractValidator validator,
        IContractStatusService statusService,
        IFileService fileService)
    {
        _db = db;
        _contractFactory = contractFactory;
        _validator = validator;
        _statusService = statusService;
        _fileService = fileService;
    }

    // GET: /Contracts  with search/filter
    public async Task<IActionResult> Index(DateTime? startFrom, DateTime? startTo, ContractStatus? status)
    {
        if (startFrom.HasValue && startTo.HasValue && startFrom.Value > startTo.Value)
        {
            TempData["Error"] = "The 'Start from' date must be on or before the 'Start to' date.";
            startFrom = null;
            startTo = null;
        }
        IQueryable<Contract> query = _db.Contracts.Include(c => c.Client);

        if (startFrom.HasValue)
            query = query.Where(c => c.StartDate >= startFrom.Value);
        if (startTo.HasValue)
            query = query.Where(c => c.StartDate <= startTo.Value);
        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        ViewBag.StartFrom = startFrom?.ToString("yyyy-MM-dd");
        ViewBag.StartTo = startTo?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        ViewBag.StatusList = new SelectList(Enum.GetValues<ContractStatus>(), status);

        var contracts = await query.OrderByDescending(c => c.StartDate).ToListAsync();
        return View(contracts);
    }

    // GET: /Contracts/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var contract = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null) return NotFound();

        ViewBag.AllowedTransitions = ContractStateTransitions.GetAllowedTransitions(contract.Status);
        return View(contract);
    }

    // GET: /Contracts/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = new SelectList(await _db.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.ServiceLevels = new SelectList(ContractFactory.ValidServiceLevels);
        return View();
    }

    // POST: /Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(15 * 1024 * 1024)] // 15 MB room over the 10 MB file limit
    public async Task<IActionResult> Create(
        int clientId,
        string serviceLevel,
        DateTime startDate,
        IFormFile? signedAgreement)
    {
        // Server-side validation
        var dateResult = _validator.ValidateDates(startDate, startDate.AddDays(1));
        var levelResult = _validator.ValidateServiceLevel(serviceLevel);

        if (!dateResult.IsValid) ModelState.AddModelError(nameof(startDate), dateResult.ErrorSummary);
        if (!levelResult.IsValid) ModelState.AddModelError(nameof(serviceLevel), levelResult.ErrorSummary);

        if (signedAgreement is null || signedAgreement.Length == 0)
        {
            ModelState.AddModelError(nameof(signedAgreement), "A signed agreement PDF is required.");
        }
        else
        {
            var fileResult = _fileService.ValidateUpload(
                signedAgreement.FileName, signedAgreement.ContentType, signedAgreement.Length);
            if (!fileResult.IsValid)
                ModelState.AddModelError(nameof(signedAgreement), fileResult.ErrorSummary);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(await _db.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", clientId);
            ViewBag.ServiceLevels = new SelectList(ContractFactory.ValidServiceLevels, serviceLevel);
            return View();
        }

        // Factory pattern — Contract with service level defaults
        var contract = _contractFactory.CreateContract(clientId, serviceLevel, startDate);

        // File upload — UUID naming, saved to server folder
        await using (var stream = signedAgreement!.OpenReadStream())
        {
            var (storagePath, originalName) = await _fileService.SaveAsync(stream, signedAgreement.FileName);
            contract.SignedAgreementPath = storagePath;
            contract.SignedAgreementOriginalName = originalName;
        }

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Contract #{contract.Id} created for client {clientId}.";
        return RedirectToAction(nameof(Details), new { id = contract.Id });
    }

    // POST: /Contracts/ChangeStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ContractStatus newStatus)
    {
        var result = await _statusService.ChangeStatusAsync(id, newStatus);

        if (result.IsValid)
            TempData["Success"] = $"Contract #{id} status changed to {newStatus}.";
        else
            TempData["Error"] = result.ErrorSummary;

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Contracts/DownloadAgreement/5
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _db.Contracts.FindAsync(id);
        if (contract is null || string.IsNullOrWhiteSpace(contract.SignedAgreementPath))
            return NotFound();

        var absolutePath = _fileService.ResolveAbsolutePath(contract.SignedAgreementPath);
        if (!System.IO.File.Exists(absolutePath))
            return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
        var downloadName = contract.SignedAgreementOriginalName ?? "agreement.pdf";
        return File(fileBytes, "application/pdf", downloadName);
    }
}