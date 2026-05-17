using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;

public class QRKodController : Controller
{
    private readonly ApplicationDbContext _context;

    public QRKodController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: QRKODS
    public async Task<IActionResult> Index()
    {
        return View(await _context.QRKodovi.ToListAsync());
    }

    // GET: QRKODS/Details/5
    public async Task<IActionResult> Details(int? qrkodid)
    {
        if (qrkodid == null)
        {
            return NotFound();
        }

        var qrkod = await _context.QRKodovi.FirstOrDefaultAsync(m => m.QRKodId == qrkodid);
        if (qrkod == null)
        {
            return NotFound();
        }

        return View(qrkod);
    }

    // GET: QRKODS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: QRKODS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] QRKod qrkod)
    {
        if (ModelState.IsValid)
        {
            _context.Add(qrkod);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(qrkod);
    }

    // GET: QRKODS/Edit/5
    public async Task<IActionResult> Edit(int? qrkodid)
    {
        if (qrkodid == null)
        {
            return NotFound();
        }

        var qrkod = await _context.QRKodovi.FindAsync(qrkodid);
        if (qrkod == null)
        {
            return NotFound();
        }
        return View(qrkod);
    }

    // POST: QRKODS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? qrkodid,
        [Bind("Id,Title,ReleaseDate,Genre,Price")] QRKod qrkod
    )
    {
        if (qrkodid != qrkod.QRKodId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(qrkod);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QRKodExists(qrkod.QRKodId))
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
        return View(qrkod);
    }

    // GET: QRKODS/Delete/5
    public async Task<IActionResult> Delete(int? qrkodid)
    {
        if (qrkodid == null)
        {
            return NotFound();
        }

        var qrkod = await _context.QRKodovi.FirstOrDefaultAsync(m => m.QRKodId == qrkodid);
        if (qrkod == null)
        {
            return NotFound();
        }

        return View(qrkod);
    }

    // POST: QRKODS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? qrkodid)
    {
        var qrkod = await _context.QRKodovi.FindAsync(qrkodid);
        if (qrkod != null)
        {
            _context.QRKodovi.Remove(qrkod);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool QRKodExists(int? qrkodid)
    {
        return _context.QRKodovi.Any(e => e.QRKodId == qrkodid);
    }
}
