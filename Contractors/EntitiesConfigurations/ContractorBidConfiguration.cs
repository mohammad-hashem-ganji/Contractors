

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
    public class ContractorBidConfiguration : IEntityTypeConfiguration<BidOfContractor>
    {
        public void Configure(EntityTypeBuilder<BidOfContractor> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Contractor)
                .WithMany(c => c.BidOfContractors)
                .HasForeignKey(x => x.ContractorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasMany(b => b.BidStatuses)
                .WithOne(c => c.ContractorBid)
                .HasForeignKey(x => x.ContractorBidId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(p => p.Project)
                .WithOne(c => c.ContractorBid)
                .HasForeignKey<Project>(x => x.ContractorBidId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
