using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Configurations;

public class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
{
    public void Configure(EntityTypeBuilder<UserStats> builder)
    {
        builder.HasKey(s => s.UserId);

        // Default values will be set in entity constructor or service layer
    }
}
