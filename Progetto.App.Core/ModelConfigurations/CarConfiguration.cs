using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.ModelConfigurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");
        builder.HasKey(c => c.LicencePlate);
        builder.Property(c => c.LicencePlate).HasMaxLength(10).IsRequired();
        builder.Property(c => c.Brand).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Model).HasMaxLength(50).IsRequired();
        builder.HasOne(c => c.ParkingSlot).WithMany().HasForeignKey(c => c.ParkingSlotId);
        builder.HasOne(c => c.Owner).WithMany().HasForeignKey(c => c.OwnerId);
    }
}
