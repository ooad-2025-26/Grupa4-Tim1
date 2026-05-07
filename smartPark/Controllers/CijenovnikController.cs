
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Models;
using smartPark.Data;

public class CijenovnikController : Controller
{
    private readonly ApplicationDbContext _context;

    public CijenovnikController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: CJENOVNIKS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Cjenovnici.ToListAsync());
    }

    // GET: CJENOVNIKS/Details/5
    public async Task<IActionResult> Details(int? cjenovnikid)
    {
        if (cjenovnikid == null)
        {
            return NotFound();
        }

        var cjenovnik = await _context.Cjenovnici
            .FirstOrDefaultAsync(m => m.CjenovnikId == cjenovnikid);
        if (cjenovnik == null)
        {
            return NotFound();
        }

        return View(cjenovnik);
    }

    // GET: CJENOVNIKS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CJENOVNIKS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Cjenovnik cjenovnik)
    {
        if (ModelState.IsValid)
        {
            _context.Add(cjenovnik);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(cjenovnik);
    }

    // GET: CJENOVNIKS/Edit/5
    public async Task<IActionResult> Edit(int? cjenovnikid)
    {
        if (cjenovnikid == null)
        {
            return NotFound();
        }

        var cjenovnik = await _context.Cjenovnici.FindAsync(cjenovnikid);
        if (cjenovnik == null)
        {
            return NotFound();
        }
        return View(cjenovnik);
    }

    // POST: CJENOVNIKS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? cjenovnikid, [Bind("Id,Title,ReleaseDate,Genre,Price")] Cjenovnik cjenovnik)
    {
        if (cjenovnikid != cjenovnik.CjenovnikId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cjenovnik);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CjenovnikExists(cjenovnik.CjenovnikId))
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
        return View(cjenovnik);
    }

    // GET: CJENOVNIKS/Delete/5
    public async Task<IActionResult> Delete(int? cjenovnikid)
    {
        if (cjenovnikid == null)
        {
            return NotFound();
        }

        var cjenovnik = await _context.Cjenovnici
            .FirstOrDefaultAsync(m => m.CjenovnikId == cjenovnikid);
        if (cjenovnik == null)
        {
            return NotFound();
        }

        return View(cjenovnik);
    }

    // POST: CJENOVNIKS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? cjenovnikid)
    {
        var cjenovnik = await _context.Cjenovnici.FindAsync(cjenovnikid);
        if (cjenovnik != null)
        {
            _context.Cjenovnici.Remove(cjenovnik);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CjenovnikExists(int? cjenovnikid)
    {
        return _context.Cjenovnici.Any(e => e.CjenovnikId == cjenovnikid);
    }
}
