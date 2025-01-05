using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PartyTavern.Models;

namespace PartyTavern.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Dodanie DbSet dla modelu Game
        public DbSet<Game> Games { get; set; }
        //Dodanie DbSet dla modelu TeamPost
        public DbSet<TeamPost> TeamPosts { get; set; }
        //Dodanie DbSet dla modelu TeamMember
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<GameProposal> GameProposals { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TeamPost>()
                .HasOne(tp => tp.Game)
                .WithMany()
                .HasForeignKey(tp => tp.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
