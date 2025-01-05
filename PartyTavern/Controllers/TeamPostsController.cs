using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PartyTavern.Data;
using PartyTavern.Models;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class TeamPostsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TeamPostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    [HttpPost]
    [Authorize]
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Leave(int postId)
    {
        var currentUserId = _userManager.GetUserId(User);

        var post = await _context.TeamPosts
            .Include(p => p.TeamMembers)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
        {
            return NotFound();
        }

        // Sprawdzamy, czy użytkownik jest twórcą drużyny
        if (post.UserId == currentUserId)
        {
            TempData["Error"] = "Twórca drużyny nie może jej opuścić.";
            return RedirectToAction("Details", new { id = postId });
        }

        var teamMember = post.TeamMembers.FirstOrDefault(tm => tm.UserId == currentUserId);
        if (teamMember != null)
        {
            _context.TeamMembers.Remove(teamMember);
            post.CurrentTeamSize--;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Pomyślnie opuściłeś drużynę.";
        }
        else
        {
            TempData["Error"] = "Nie jesteś członkiem tej drużyny.";
        }

        return RedirectToAction("Details", new { id = postId });
    }

    public async Task<IActionResult> Details(int id, string returnUrl = null)
    {
        var post = await _context.TeamPosts
            .Include(p => p.Game)
            .Include(p => p.TeamMembers).ThenInclude(tm => tm.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        ViewData["CurrentUserId"] = _userManager.GetUserId(User);
        ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index", "TeamPosts");
        return View(post);
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

    [Authorize]
    public async Task<IActionResult> MyTeams(bool showExpired = false)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IQueryable<TeamPost> teamPostsQuery = _context.TeamPosts
            .Where(p => p.TeamMembers.Any(tm => tm.UserId == currentUserId));

        if (!showExpired)
        {
            teamPostsQuery = teamPostsQuery.Where(p => p.NeededBy >= DateTime.Now); // Pokaż tylko aktywne drużyny
        }
        else
        {
            teamPostsQuery = teamPostsQuery.Where(p => p.NeededBy < DateTime.Now); // Pokaż tylko przedawnione drużyny
        }

        var teamPosts = await teamPostsQuery
            .Include(p => p.Game)
            .Include(p => p.TeamMembers).ThenInclude(tm => tm.User)
            .ToListAsync();

        ViewData["ShowExpired"] = showExpired; // Ustawiamy wartość dla widoku

        return View(teamPosts);
    }

    // Akcja do dodawania komentarza
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult AddComment(int postId, string content)
    {
        var post = _context.TeamPosts.Find(postId);

        if (post == null)
        {
            return NotFound();
        }

        if (post.NeededBy < DateTime.Now)
        {
            TempData["ErrorMessage"] = "Nie można dodawać komentarzy do przedawnionych postów.";
            return RedirectToAction("Details", new { id = postId });
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Komentarz nie może być pusty.";
            return RedirectToAction("Details", new { id = postId });
        }

        var currentUserId = _userManager.GetUserId(User);

        var comment = new Comment
        {
            Content = content,
            CreatedAt = DateTime.Now,
            UserId = currentUserId,
            PostId = postId
        };

        _context.Comments.Add(comment);
        _context.SaveChanges();

        return RedirectToAction("Details", new { id = postId });
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int postId)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Sprawdź, czy użytkownik jest właścicielem komentarza lub adminem
        if (comment.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = postId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditComment(int commentId)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Sprawdź, czy użytkownik jest właścicielem komentarza
        if (comment.UserId != currentUserId)
        {
            return Forbid();
        }

        return View(comment);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(int commentId, string content)
    {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Sprawdź, czy użytkownik jest właścicielem komentarza
        if (comment.UserId != currentUserId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError("", "Komentarz nie może być pusty.");
            return View(comment);
        }

        comment.Content = content;
        comment.wasEdited = true;

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = comment.PostId });
    }


    private bool TeamPostExists(int id)
    {
        return _context.TeamPosts.Any(e => e.Id == id);
    }


}
