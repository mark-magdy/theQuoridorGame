using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Configurations;

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.HasKey(gp => gp.Id);

        builder.HasIndex(gp => new { gp.GameId, gp.UserId });
        builder.HasIndex(gp => new { gp.GameId, gp.PlayerId });
    }
}
