

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
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);
            builder.HasOne(p => p.ApplicationUser)
            .WithOne(c => c.Client)
            .HasForeignKey<Client>(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
