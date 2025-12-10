using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.GameStateJson)
            .IsRequired();

        builder.Property(g => g.SettingsJson)
            .IsRequired();

        builder.Property(g => g.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(g => g.JoinCode)
            .HasMaxLength(8);

        builder.HasIndex(g => g.JoinCode);
        builder.HasIndex(g => g.Status);
        builder.HasIndex(g => g.CreatedAt);

        // One-to-Many with GamePlayers
        builder.HasMany(g => g.GamePlayers)
            .WithOne(gp => gp.Game)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-Many with GameMoves
        builder.HasMany(g => g.Moves)
            .WithOne(m => m.Game)
            .HasForeignKey(m => m.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
