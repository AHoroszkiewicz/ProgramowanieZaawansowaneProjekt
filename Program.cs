using Microsoft.EntityFrameworkCore; // Upewnij si�, �e to jest dodane
using PartyTavern.Data; // Upewnij si�, �e to jest poprawna przestrze� nazw

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Dodaj konfiguracj� DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Dodaj tras� dla TestController
app.MapControllerRoute(
    name: "test",
    pattern: "Test/{action=CreateTestUser}/{id?}",
    defaults: new { controller = "Test", action = "CreateTestUser" } // Ustawienia domy�lne
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
