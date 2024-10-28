using Contractors.Entites;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Contractors.EntitiesConfigurations
{
    public class BidStatusConfiguration : IEntityTypeConfiguration<BidStatus>
    {
        public void Configure(EntityTypeBuilder<BidStatus> builder)
        {
            builder.HasKey(b => b.Id);
        }


    }
}
