using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;

public class ParkingMjestoController : Controller
{
    private readonly ApplicationDbContext _context;

    public ParkingMjestoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PARKINGMJESTOS
    public async Task<IActionResult> Index()
    {
        return View(await _context.ParkingMjesta.ToListAsync());
    }

    // GET: PARKINGMJESTOS/Details/5
    public async Task<IActionResult> Details(int? parkingmjestoid)
    {
        if (parkingmjestoid == null)
        {
            return NotFound();
        }

        var parkingmjesto = await _context.ParkingMjesta.FirstOrDefaultAsync(m =>
            m.ParkingMjestoId == parkingmjestoid
        );
        if (parkingmjesto == null)
        {
            return NotFound();
        }

        return View(parkingmjesto);
    }

    // GET: PARKINGMJESTOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PARKINGMJESTOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,ReleaseDate,Genre,Price")] ParkingMjesto parkingmjesto
    )
    {
        if (ModelState.IsValid)
        {
            _context.Add(parkingmjesto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(parkingmjesto);
    }

    // GET: PARKINGMJESTOS/Edit/5
    public async Task<IActionResult> Edit(int? parkingmjestoid)
    {
        if (parkingmjestoid == null)
        {
            return NotFound();
        }

        var parkingmjesto = await _context.ParkingMjesta.FindAsync(parkingmjestoid);
        if (parkingmjesto == null)
        {
            return NotFound();
        }
        return View(parkingmjesto);
    }

    // POST: PARKINGMJESTOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? parkingmjestoid,
        [Bind("Id,Title,ReleaseDate,Genre,Price")] ParkingMjesto parkingmjesto
    )
    {
        if (parkingmjestoid != parkingmjesto.ParkingMjestoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(parkingmjesto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkingMjestoExists(parkingmjesto.ParkingMjestoId))
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
        return View(parkingmjesto);
    }

    // GET: PARKINGMJESTOS/Delete/5
    public async Task<IActionResult> Delete(int? parkingmjestoid)
    {
        if (parkingmjestoid == null)
        {
            return NotFound();
        }

        var parkingmjesto = await _context.ParkingMjesta.FirstOrDefaultAsync(m =>
            m.ParkingMjestoId == parkingmjestoid
        );
        if (parkingmjesto == null)
        {
            return NotFound();
        }

        return View(parkingmjesto);
    }

    // POST: PARKINGMJESTOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? parkingmjestoid)
    {
        var parkingmjesto = await _context.ParkingMjesta.FindAsync(parkingmjestoid);
        if (parkingmjesto != null)
        {
            _context.ParkingMjesta.Remove(parkingmjesto);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ParkingMjestoExists(int? parkingmjestoid)
    {
        return _context.ParkingMjesta.Any(e => e.ParkingMjestoId == parkingmjestoid);
    }
}
