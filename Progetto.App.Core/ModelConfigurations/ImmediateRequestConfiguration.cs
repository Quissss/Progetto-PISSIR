using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

public class ImmediateRequestConfiguration
{
    public void Configure(EntityTypeBuilder<ImmediateRequest> builder)
    {
        builder.ToTable("ImmediateRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RequestDate).IsRequired();
        builder.Property(r => r.RequestedChargeLevel).HasDefaultValue(100).IsRequired();
        builder.HasOne(r => r.ParkingSlot).WithMany().HasForeignKey(r => r.ParkingSlotId);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
        builder.Property(r => r.FromReservation).HasDefaultValue(false).IsRequired();
    }
}
