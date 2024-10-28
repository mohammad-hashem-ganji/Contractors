
using Contractors.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.EntitiesConfigurations
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasMany(r => r.Requests)
            .WithOne(r => r.Region)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        }
    }
}
