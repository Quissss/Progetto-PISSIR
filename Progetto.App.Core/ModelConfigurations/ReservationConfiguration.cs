using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the Reservation entity (table) in the database (EF Core)
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ReservationTime).IsRequired();
        builder.Property(r => r.RequestDate).IsRequired();
        builder.Property(r => r.RequestedChargeLevel).IsRequired();
        builder.HasOne(r => r.Parking).WithMany().HasForeignKey(r => r.ParkingId);
        builder.HasOne(r => r.Car).WithMany().HasForeignKey(r => r.CarPlate).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
        builder.Property(r => r.CarIsInside).IsRequired();
    }
}
