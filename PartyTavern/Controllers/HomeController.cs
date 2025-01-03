using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PartyTavern.Data;
using PartyTavern.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace PartyTavern.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Pobierz bie¿¹cego u¿ytkownika
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            // Przeka¿ dane do widoku za pomoc¹ ViewData
            ViewData["IsAdmin"] = isAdmin;

            return View();
        }

        // Nowa akcja dla wyœwietlania gier - tylko dla administratora
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Games()
        {
            var games = await _context.Games.ToListAsync(); // Pobranie gier z bazy danych
            return View(games); // Przekazanie listy gier do widoku
        }

        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
