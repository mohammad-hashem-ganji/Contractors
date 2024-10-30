

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
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasMany(p => p.ProjectStatuses)
                .WithOne(p => p.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            builder.HasOne(p => p.Contractor)
                .WithMany(p => p.ContractorProjects)
                .HasForeignKey(p => p.ContractorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            builder.HasOne(p => p.Client)
                .WithMany(p => p.ClientProjects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
