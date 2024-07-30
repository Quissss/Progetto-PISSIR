using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the Car entity (table) in the database (EF Core)
/// </summary>
public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");
        builder.HasKey(c => c.LicencePlate);
        builder.Property(c => c.LicencePlate).HasMaxLength(10).IsRequired();
        builder.Property(c => c.Brand).HasMaxLength(50);
        builder.Property(c => c.Model).HasMaxLength(50);
        builder.Property(c => c.IsElectric).HasDefaultValue(true).IsRequired();
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.ParkingSlotId).IsRequired(false);
        builder.HasOne(c => c.Parking).WithMany().HasForeignKey(c => c.ParkingId).IsRequired(false);
        builder.HasOne(c => c.Owner).WithMany().HasForeignKey(c => c.OwnerId).IsRequired();
    }
}
