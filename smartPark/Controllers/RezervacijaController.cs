
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Models;
using smartPark.Data;

public class RezervacijaController : Controller
{
    private readonly ApplicationDbContext _context;

    public RezervacijaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: REZERVACIJAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Rezervacije.ToListAsync());
    }

    // GET: REZERVACIJAS/Details/5
    public async Task<IActionResult> Details(int? rezervacijaid)
    {
        if (rezervacijaid == null)
        {
            return NotFound();
        }

        var rezervacija = await _context.Rezervacije
            .FirstOrDefaultAsync(m => m.RezervacijaId == rezervacijaid);
        if (rezervacija == null)
        {
            return NotFound();
        }

        return View(rezervacija);
    }

    // GET: REZERVACIJAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: REZERVACIJAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Rezervacija rezervacija)
    {
        if (ModelState.IsValid)
        {
            _context.Add(rezervacija);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(rezervacija);
    }

    // GET: REZERVACIJAS/Edit/5
    public async Task<IActionResult> Edit(int? rezervacijaid)
    {
        if (rezervacijaid == null)
        {
            return NotFound();
        }

        var rezervacija = await _context.Rezervacije.FindAsync(rezervacijaid);
        if (rezervacija == null)
        {
            return NotFound();
        }
        return View(rezervacija);
    }

    // POST: REZERVACIJAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? rezervacijaid, [Bind("Id,Title,ReleaseDate,Genre,Price")] Rezervacija rezervacija)
    {
        if (rezervacijaid != rezervacija.RezervacijaId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(rezervacija);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RezervacijaExists(rezervacija.RezervacijaId))
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
        return View(rezervacija);
    }

    // GET: REZERVACIJAS/Delete/5
    public async Task<IActionResult> Delete(int? rezervacijaid)
    {
        if (rezervacijaid == null)
        {
            return NotFound();
        }

        var rezervacija = await _context.Rezervacije
            .FirstOrDefaultAsync(m => m.RezervacijaId == rezervacijaid);
        if (rezervacija == null)
        {
            return NotFound();
        }

        return View(rezervacija);
    }

    // POST: REZERVACIJAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? rezervacijaid)
    {
        var rezervacija = await _context.Rezervacije.FindAsync(rezervacijaid);
        if (rezervacija != null)
        {
            _context.Rezervacije.Remove(rezervacija);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RezervacijaExists(int? rezervacijaid)
    {
        return _context.Rezervacije.Any(e => e.RezervacijaId == rezervacijaid);
    }
}
