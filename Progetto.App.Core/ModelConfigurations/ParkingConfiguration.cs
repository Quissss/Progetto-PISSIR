using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the Parking entity (table) in the database (EF Core)
/// </summary>
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
        builder.Property(p => p.EnergyCostPerKw).HasColumnType("decimal(5, 2)").IsRequired();
        builder.Property(p => p.StopCostPerMinute).HasColumnType("decimal(5, 2)").IsRequired();
        builder.HasMany(p => p.ParkingSlots).WithOne().HasForeignKey(ps => ps.ParkingId).OnDelete(DeleteBehavior.Cascade);
    }
}
