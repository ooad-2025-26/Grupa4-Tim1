using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models;

[Authorize(Roles = "Vozac")]
public class VozacController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Korisnik> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public VozacController(
        ApplicationDbContext context,
        UserManager<Korisnik> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Korisnici()
    {
        var korisnici = await _userManager.Users.ToListAsync();
        return View(korisnici);
    }
}
