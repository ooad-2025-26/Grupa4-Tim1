using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;

public class ParkingController : Controller
{
    private readonly ApplicationDbContext _context;

    public ParkingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PARKINGS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Parkinzi.ToListAsync());
    }

    // GET: PARKINGS/Details/5
    public async Task<IActionResult> Details(int? parkingid)
    {
        if (parkingid == null)
        {
            return NotFound();
        }

        var parking = await _context.Parkinzi.FirstOrDefaultAsync(m => m.ParkingId == parkingid);
        if (parking == null)
        {
            return NotFound();
        }

        return View(parking);
    }

    // GET: PARKINGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PARKINGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Parking parking)
    {
        if (ModelState.IsValid)
        {
            _context.Add(parking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(parking);
    }

    // GET: PARKINGS/Edit/5
    public async Task<IActionResult> Edit(int? parkingid)
    {
        if (parkingid == null)
        {
            return NotFound();
        }

        var parking = await _context.Parkinzi.FindAsync(parkingid);
        if (parking == null)
        {
            return NotFound();
        }
        return View(parking);
    }

    // POST: PARKINGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? parkingid,
        [Bind("Id,Title,ReleaseDate,Genre,Price")] Parking parking
    )
    {
        if (parkingid != parking.ParkingId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(parking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkingExists(parking.ParkingId))
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
        return View(parking);
    }

    // GET: PARKINGS/Delete/5
    public async Task<IActionResult> Delete(int? parkingid)
    {
        if (parkingid == null)
        {
            return NotFound();
        }

        var parking = await _context.Parkinzi.FirstOrDefaultAsync(m => m.ParkingId == parkingid);
        if (parking == null)
        {
            return NotFound();
        }

        return View(parking);
    }

    // POST: PARKINGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? parkingid)
    {
        var parking = await _context.Parkinzi.FindAsync(parkingid);
        if (parking != null)
        {
            _context.Parkinzi.Remove(parking);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ParkingExists(int? parkingid)
    {
        return _context.Parkinzi.Any(e => e.ParkingId == parkingid);
    }
}
