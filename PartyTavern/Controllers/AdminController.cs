using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartyTavern.Data;
using PartyTavern.Models;

namespace PartyTavern.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Akcja dodawania nowej gry
        public IActionResult AddGame()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGame(Game game)
        {
            if (ModelState.IsValid)
            {
                _context.Games.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction("Games");
            }
            return View(game);
        }

        // Akcja wyświetlania dostępnych gier
        public async Task<IActionResult> Games()
        {
            var games = await _context.Games.ToListAsync(); // Pobranie gier z bazy danych
            return View(games); // Przekazanie gier do widoku
        }

        // Akcja edytowania gry
        public async Task<IActionResult> EditGame(int id)
        {
            var game = await _context.Games.FindAsync(id); // Pobranie gry z bazy danych
            if (game == null)
            {
                return NotFound(); // Jeśli gry nie ma, zwróć błąd 404
            }
            return View(game); // Przekazanie gry do widoku edycji
        }

        [HttpPost]
        public async Task<IActionResult> EditGame(int id, Game game)
        {
            if (id != game.Id)
            {
                return NotFound(); // Jeśli id gry się nie zgadza, zwróć błąd 404
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game); // Zaktualizowanie gry w bazie danych
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Games.Any(g => g.Id == id))
                    {
                        return NotFound(); // Jeśli gry nie ma w bazie, zwróć błąd 404
                    }
                    else
                    {
                        throw; // Inny błąd, rzuć wyjątek
                    }
                }
                return RedirectToAction("Games"); // Przekierowanie na stronę z grami
            }
            return View(game); // Jeśli formularz jest niepoprawny, wróć do formularza
        }

        // Akcja usuwania gry
        // Nowa akcja dla usuwania gry
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id); // Pobranie gry z bazy danych
            if (game == null)
            {
                return NotFound(); // Jeśli gry nie ma, zwróć błąd 404
            }
            return View(game); // Przekazanie gry do widoku
        }

        [HttpPost, ActionName("DeleteGame")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id); // Pobranie gry z bazy danych
            if (game == null)
            {
                return NotFound(); // Jeśli gry nie ma, zwróć błąd 404
            }

            _context.Games.Remove(game); // Usunięcie gry z bazy danych
            await _context.SaveChangesAsync();
            return RedirectToAction("Games"); // Przekierowanie na stronę z grami
        }

    }
}
