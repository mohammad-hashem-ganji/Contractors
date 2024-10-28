using Contractors.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contractors.EntitiesConfigurations
{
    public class RejectConfiguration : IEntityTypeConfiguration<Reject>
    {
        public void Configure(EntityTypeBuilder<Reject> builder)
        {
            builder.HasKey(r => r.Id);
            builder.HasOne(r => r.Request)
                .WithMany(r => r.Rejects)
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
