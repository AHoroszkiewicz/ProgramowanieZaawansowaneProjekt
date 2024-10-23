using Microsoft.AspNetCore.Mvc;
using PartyTavern.Data;
using PartyTavern.Models;

public class TestController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult CreateTestUser()
    {
        var user = new User { Username = "TestUser", Password = "TestPassword" }; // Użyj prostego hasła dla testu
        _context.Users.Add(user);
        _context.SaveChanges(); // Zapisz zmiany w bazie danych
        return Ok("Test user created");
    }
}
