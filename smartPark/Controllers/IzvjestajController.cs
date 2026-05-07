
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Models;
using smartPark.Data;

public class IzvjestajController : Controller
{
    private readonly ApplicationDbContext _context;

    public IzvjestajController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: IZVJESTAJS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Izvjestaji.ToListAsync());
    }

    // GET: IZVJESTAJS/Details/5
    public async Task<IActionResult> Details(int? izvjestajid)
    {
        if (izvjestajid == null)
        {
            return NotFound();
        }

        var izvjestaj = await _context.Izvjestaji
            .FirstOrDefaultAsync(m => m.IzvjestajId == izvjestajid);
        if (izvjestaj == null)
        {
            return NotFound();
        }

        return View(izvjestaj);
    }

    // GET: IZVJESTAJS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: IZVJESTAJS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Izvjestaj izvjestaj)
    {
        if (ModelState.IsValid)
        {
            _context.Add(izvjestaj);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(izvjestaj);
    }

    // GET: IZVJESTAJS/Edit/5
    public async Task<IActionResult> Edit(int? izvjestajid)
    {
        if (izvjestajid == null)
        {
            return NotFound();
        }

        var izvjestaj = await _context.Izvjestaji.FindAsync(izvjestajid);
        if (izvjestaj == null)
        {
            return NotFound();
        }
        return View(izvjestaj);
    }

    // POST: IZVJESTAJS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? izvjestajid, [Bind("Id,Title,ReleaseDate,Genre,Price")] Izvjestaj izvjestaj)
    {
        if (izvjestajid != izvjestaj.IzvjestajId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(izvjestaj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IzvjestajExists(izvjestaj.IzvjestajId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(izvjestaj);
    }

    // GET: IZVJESTAJS/Delete/5
    public async Task<IActionResult> Delete(int? izvjestajid)
    {
        if (izvjestajid == null)
        {
            return NotFound();
        }

        var izvjestaj = await _context.Izvjestaji
            .FirstOrDefaultAsync(m => m.IzvjestajId == izvjestajid);
        if (izvjestaj == null)
        {
            return NotFound();
        }

        return View(izvjestaj);
    }

    // POST: IZVJESTAJS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? izvjestajid)
    {
        var izvjestaj = await _context.Izvjestaji.FindAsync(izvjestajid);
        if (izvjestaj != null)
        {
            _context.Izvjestaji.Remove(izvjestaj);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool IzvjestajExists(int? izvjestajid)
    {
        return _context.Izvjestaji.Any(e => e.IzvjestajId == izvjestajid);
    }
}
