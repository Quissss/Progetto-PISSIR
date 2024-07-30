using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the ParkingSlot entity (table) in the database (EF Core)
/// </summary>
public class ParkingSlotConfiguration : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable("ParkingSlots");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Number).IsRequired();
        builder.Property(p => p.Status).IsRequired()
            .HasConversion(
                value => (int)value,
                value => Enum.Parse<ParkingSlotStatus>(value.ToString())
            );
        builder.HasOne(p => p.Parking).WithMany(p => p.ParkingSlots).HasForeignKey(p => p.ParkingId);
    }
}
