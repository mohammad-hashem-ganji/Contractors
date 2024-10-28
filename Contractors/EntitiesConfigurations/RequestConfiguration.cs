

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
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(m => m.Id);

            builder
                .HasMany(r => r.FileAttachments)
                .WithOne(f => f.Request)
                .HasForeignKey(f => f.RequestId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(b => b.BidOfContractors)
                .WithOne(r => r.Request)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder
                .HasOne(c => c.Client)
                .WithMany(r => r.Requests)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder
                .HasMany(s => s.RequestStatuses)
                .WithOne(r => r.Request)
                .HasForeignKey(s => s.RequestId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
                
        }
    }
}
