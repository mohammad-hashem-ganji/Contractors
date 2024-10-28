

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
    public class ContractorConfiguration : IEntityTypeConfiguration<Contractor>
    {
        public void Configure(EntityTypeBuilder<Contractor> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(p => p.ApplicationUser)
            .WithOne(c => c.Contractor)
            .HasForeignKey<Contractor>(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
