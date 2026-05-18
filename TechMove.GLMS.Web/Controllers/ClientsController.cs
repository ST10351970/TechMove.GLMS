using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Entities;

namespace TechMove.GLMS.Web.Controllers;

public class ClientsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ClientsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Clients
    public async Task<IActionResult> Index()
    {
        var clients = await _db.Clients
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(clients);
    }

    // GET: /Clients/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var client = await _db.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null) return NotFound();
        return View(client);
    }

    // GET: /Clients/Create
    public IActionResult Create() => View();

    // POST: /Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
    {
        if (!ModelState.IsValid) return View(client);

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Client '{client.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clients/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client is null) return NotFound();
        return View(client);
    }

    // POST: /Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactDetails,Region")] Client client)
    {
        if (id != client.Id) return BadRequest();
        if (!ModelState.IsValid) return View(client);

        _db.Update(client);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Client '{client.Name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clients/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null) return NotFound();

        if (client.Contracts.Any())
        {
            TempData["Error"] =
                $"Client '{client.Name}' cannot be deleted because they have " +
                $"{client.Contracts.Count} contract(s) attached. " +
                $"Existing contracts and their service requests must be preserved for audit purposes.";
            return RedirectToAction(nameof(Index));
        }

        return View(client);
    }

    // POST: /Clients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _db.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null) return NotFound();

        // Defence in depth: re-check on POST in case a contract was added between GET and POST
        if (client.Contracts.Any())
        {
            TempData["Error"] =
                $"Client '{client.Name}' cannot be deleted because they have contracts attached.";
            return RedirectToAction(nameof(Index));
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Client '{client.Name}' deleted.";
        return RedirectToAction(nameof(Index));
    }
}