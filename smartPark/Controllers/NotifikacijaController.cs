
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Models;
using smartPark.Data;

public class NotifikacijaController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotifikacijaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: NOTIFIKACIJAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Notifikacije.ToListAsync());
    }

    // GET: NOTIFIKACIJAS/Details/5
    public async Task<IActionResult> Details(int? notifikacijaid)
    {
        if (notifikacijaid == null)
        {
            return NotFound();
        }

        var notifikacija = await _context.Notifikacije
            .FirstOrDefaultAsync(m => m.NotifikacijaId == notifikacijaid);
        if (notifikacija == null)
        {
            return NotFound();
        }

        return View(notifikacija);
    }

    // GET: NOTIFIKACIJAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: NOTIFIKACIJAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Notifikacija notifikacija)
    {
        if (ModelState.IsValid)
        {
            _context.Add(notifikacija);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(notifikacija);
    }

    // GET: NOTIFIKACIJAS/Edit/5
    public async Task<IActionResult> Edit(int? notifikacijaid)
    {
        if (notifikacijaid == null)
        {
            return NotFound();
        }

        var notifikacija = await _context.Notifikacije.FindAsync(notifikacijaid);
        if (notifikacija == null)
        {
            return NotFound();
        }
        return View(notifikacija);
    }

    // POST: NOTIFIKACIJAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? notifikacijaid, [Bind("Id,Title,ReleaseDate,Genre,Price")] Notifikacija notifikacija)
    {
        if (notifikacijaid != notifikacija.NotifikacijaId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(notifikacija);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotifikacijaExists(notifikacija.NotifikacijaId))
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
        return View(notifikacija);
    }

    // GET: NOTIFIKACIJAS/Delete/5
    public async Task<IActionResult> Delete(int? notifikacijaid)
    {
        if (notifikacijaid == null)
        {
            return NotFound();
        }

        var notifikacija = await _context.Notifikacije
            .FirstOrDefaultAsync(m => m.NotifikacijaId == notifikacijaid);
        if (notifikacija == null)
        {
            return NotFound();
        }

        return View(notifikacija);
    }

    // POST: NOTIFIKACIJAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? notifikacijaid)
    {
        var notifikacija = await _context.Notifikacije.FindAsync(notifikacijaid);
        if (notifikacija != null)
        {
            _context.Notifikacije.Remove(notifikacija);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NotifikacijaExists(int? notifikacijaid)
    {
        return _context.Notifikacije.Any(e => e.NotifikacijaId == notifikacijaid);
    }
}
