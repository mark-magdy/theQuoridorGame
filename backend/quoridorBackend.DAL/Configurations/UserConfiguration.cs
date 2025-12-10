using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // One-to-One with UserStats
        builder.HasOne(u => u.Stats)
            .WithOne(s => s.User)
            .HasForeignKey<UserStats>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-Many with Games (created games)
        builder.HasMany(u => u.CreatedGames)
            .WithOne(g => g.Creator)
            .HasForeignKey(g => g.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-Many with GamePlayers
        builder.HasMany(u => u.GameParticipations)
            .WithOne(gp => gp.User)
            .HasForeignKey(gp => gp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
