using Microsoft.EntityFrameworkCore;

namespace Real_time_Leaderboard.Models
{
    public class AppDbContext : DbContext
    {
        // The constructor allows Program.cs to pass in the PostgreSQL connection string
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Each DbSet represents a table in your PostgreSQL database
        public DbSet<Player> Players { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<ScoreEntry> ScoreEntries { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Find all tracked entities in the current transaction that inherit from BaseEntity
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                    // TODO: Set entry.Entity.CreatedBy once JWT authentication is implemented
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;

                    // TODO: Set entry.Entity.UpdatedBy once JWT authentication is implemented
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call the base method first
            base.OnModelCreating(modelBuilder);

            // Seed the default Role data
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    RoleName = "Player"
                },
                new Role
                {
                    RoleId = 2,
                    RoleName = "Admin" // Might as well add this while we are here!
                }
            );
        }
    }
}
