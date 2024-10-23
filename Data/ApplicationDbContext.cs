using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PartyTaveern.Models;
using PartyTavern.Models;

namespace PartyTavern.Data


{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Przykład DbSetów - dostosuj do swoich potrzeb
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
