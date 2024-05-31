using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.ModelConfigurations;

public class ParkingConfiguration : IEntityTypeConfiguration<Parking>
{
    public void Configure(EntityTypeBuilder<Parking> builder)
    {
        builder.ToTable("Parkings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Address).HasMaxLength(100).IsRequired();
        builder.Property(p => p.City).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Province).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PostalCode).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Country).HasMaxLength(50).IsRequired();
        builder.Property(p => p.EnergyCostPerMinute).HasColumnType("decimal(5, 2)").IsRequired();
        builder.HasMany(p => p.ParkingSlots).WithOne().HasForeignKey(ps => ps.Id);
    }
}
