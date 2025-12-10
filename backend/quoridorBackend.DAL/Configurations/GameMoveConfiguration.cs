using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Configurations;

public class GameMoveConfiguration : IEntityTypeConfiguration<GameMove>
{
    public void Configure(EntityTypeBuilder<GameMove> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MoveType)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(m => m.MoveDataJson)
            .IsRequired();

        builder.HasIndex(m => new { m.GameId, m.MoveNumber });
        builder.HasIndex(m => m.Timestamp);
    }
}
