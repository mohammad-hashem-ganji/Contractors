using Contractors.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contractors.EntitiesConfigurations
{
    public class LastLoginHistoryConfiguration : IEntityTypeConfiguration<LastLoginHistory>
    {
        public void Configure(EntityTypeBuilder<LastLoginHistory> builder)
        {
            builder.HasKey(v => v.Id);
            builder.HasOne(v => v.ApplicationUser)
                .WithMany(a => a.LastLoginHistories)
                .HasForeignKey(v => v.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
