using Microsoft.EntityFrameworkCore;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Data;

public class QuoridorDbContext : DbContext
{
    public QuoridorDbContext(DbContextOptions<QuoridorDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserStats> UserStats { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    public DbSet<GameMove> GameMoves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply configurations from separate files
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuoridorDbContext).Assembly);
    }
}
