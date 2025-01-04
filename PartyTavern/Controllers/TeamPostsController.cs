using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PartyTavern.Data;
using PartyTavern.Models;

public class TeamPostsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TeamPostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /TeamPosts/Create
    public IActionResult Create()
    {
        ViewBag.Games = _context.Games.ToList(); // Lista gier do wyboru
        return View();
    }

    // POST: /TeamPosts/Create
    [HttpPost]
    public async Task<IActionResult> Create(TeamPost post)
    {
        if (ModelState.IsValid)
        {
            post.UserId = _userManager.GetUserId(User);
            post.CreatedAt = DateTime.Now;

            Console.WriteLine($"Current User ID: {_userManager.GetUserId(User)}");
            Console.WriteLine($"User ID: {post.UserId}");

            // Dodaj członka do drużyny
            post.TeamMembers.Add(new TeamMember
            {
                UserId = post.UserId,
                TeamPostId = post.Id
            });
            post.CurrentTeamSize++;

            // Dodaj post do bazy danych
            _context.TeamPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        ViewBag.Games = _context.Games.ToList();
        return View(post);
    }

    // GET: /TeamPosts/Index
    public async Task<IActionResult> Index(bool showExpired = false)
    {
        var userId = _userManager.GetUserId(User); // Uzyskaj UserId aktualnie zalogowanego użytkownika

        IQueryable<TeamPost> query;

        if (showExpired)
        {
            // Pobierz posty z przeszłości
            query = _context.TeamPosts
                            .Include(p => p.Game)
                            .Include(p => p.TeamMembers)
                            .ThenInclude(tm => tm.User)
                            .Where(p => p.NeededBy < DateTime.Now); // Posty przedawnione
        }
        else
        {
            // Pobierz aktywne posty
            query = _context.TeamPosts
                            .Include(p => p.Game)
                            .Include(p => p.TeamMembers)
                            .ThenInclude(tm => tm.User)
                            .Where(p => p.NeededBy >= DateTime.Now); // Posty w przyszłości
        }

        var posts = await query.ToListAsync();

        // Przekazujemy ID użytkownika i czy pokazujemy przedawnione posty
        ViewData["CurrentUserId"] = userId;
        ViewData["ShowExpired"] = showExpired;

        return View(posts);
    }

    [HttpPost]
    public async Task<IActionResult> Join(int postId)
    {
        var teamPost = await _context.TeamPosts
            .Include(p => p.TeamMembers)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (teamPost == null)
        {
            return NotFound();
        }

        // Sprawdź, czy drużyna jest przedawniona
        if (teamPost.NeededBy < DateTime.Now)
        {
            ModelState.AddModelError(string.Empty, "Nie można dołączyć do przedawnionej drużyny.");
            return RedirectToAction("Index");
        }

        var userId = _userManager.GetUserId(User);

        // Sprawdź, czy użytkownik już dołączył do tej drużyny
        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamPostId == postId && tm.UserId == userId);

        if (existingMember != null)
        {
            ModelState.AddModelError(string.Empty, "You have already joined this team.");
            return RedirectToAction("Index");
        }

        // Sprawdź, czy drużyna nie jest pełna
        if (teamPost.CurrentTeamSize >= teamPost.TeamSize)
        {
            ModelState.AddModelError(string.Empty, "The team is already full.");
            return RedirectToAction("Index");
        }

        // Dodaj członka do drużyny
        _context.TeamMembers.Add(new TeamMember
        {
            UserId = userId,
            TeamPostId = postId
        });

        // Zaktualizuj rozmiar drużyny
        teamPost.CurrentTeamSize++;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.TeamPosts
            .Include(p => p.Game)  // Załaduj powiązaną grę
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        // Pobierz listę gier do wyboru w formularzu
        ViewBag.Games = new SelectList(await _context.Games.ToListAsync(), "Id", "Name");

        // Sprawdzamy, czy użytkownik jest administratorem lub twórcą posta
        var userId = _userManager.GetUserId(User);
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Unauthorized(); // Jeśli nie jest twórcą ani adminem, zwróć błąd
        }

        // Przekaż post do widoku
        return View(post);
    }

    // POST: /TeamPosts/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TeamPost post)
    {
        if (id != post.Id)
        {
            return NotFound();
        }

        // Pobierz drużynę z bazy danych
        var teamPost = await _context.TeamPosts
            .Include(p => p.TeamMembers)  // Wczytaj członków drużyny
            .FirstOrDefaultAsync(p => p.Id == id);

        if (teamPost == null)
        {
            return NotFound();
        }

        // Sprawdzanie, czy proponowany rozmiar drużyny jest wystarczający
        if (post.TeamSize < teamPost.TeamMembers.Count)
        {
            ModelState.AddModelError("TeamSize", "Rozmiar drużyny nie może być mniejszy niż liczba zapisanych członków.");
            return View(post);  // Zwróć formularz z błędem
        }

        teamPost.TeamSize = post.TeamSize;

        // Zaktualizuj inne właściwości drużyny
        teamPost.Description = post.Description;
        teamPost.NeededBy = post.NeededBy;
        teamPost.GameId = post.GameId;

        // Zaktualizuj drużynę w bazie danych
        _context.Update(teamPost);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    // GET: /TeamPosts/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.TeamPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Sprawdzamy, czy użytkownik jest administratorem lub twórcą posta
        var userId = _userManager.GetUserId(User);
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Unauthorized(); // Jeśli nie jest twórcą ani adminem, zwróć błąd
        }

        return View(post);
    }

    // POST: /TeamPosts/Delete/{id}
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> Delete(int id, TeamPost post)
    {
        if (post == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        Console.WriteLine($"Current User ID: {userId}");
        Console.WriteLine($"Post User ID: {post.UserId}");

        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            Console.WriteLine("Unauthorized access attempt.");
            return Unauthorized();
        }


        _context.TeamPosts.Remove(post);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    private bool TeamPostExists(int id)
    {
        return _context.TeamPosts.Any(e => e.Id == id);
    }

}
