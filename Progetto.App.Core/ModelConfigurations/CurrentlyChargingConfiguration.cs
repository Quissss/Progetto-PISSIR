using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the CurrentlyCharging entity (table) in the database (EF Core)
/// </summary>
public class CurrentlyChargingConfiguration : IEntityTypeConfiguration<CurrentlyCharging>
{
    public void Configure(EntityTypeBuilder<CurrentlyCharging> builder)
    {
        builder.ToTable("CurrentlyCharging");
        builder.HasKey(cc => cc.Id);
        builder.Property(cc => cc.StartChargingTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(cc => cc.EndChargingTime).IsRequired(false);
        builder.HasOne(cc => cc.Car).WithMany().HasForeignKey(cc => cc.CarPlate).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(cc => cc.MwBot).WithMany().HasForeignKey(cc => cc.MwBotId);
        builder.HasOne(cc => cc.User).WithMany().HasForeignKey(cc => cc.UserId);
        builder.HasOne(cc => cc.ParkingSlot).WithMany().HasForeignKey(cc => cc.ParkingSlotId);
        builder.HasOne(cc => cc.ImmediateRequest).WithMany().HasForeignKey(cc => cc.ImmediateRequestId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(cc => cc.ImmediateRequestId).IsRequired(false);
        builder.Property(cc => cc.StartChargePercentage).HasColumnType("decimal(5, 2)");
        builder.Property(cc => cc.CurrentChargePercentage).HasColumnType("decimal(5, 2)");
        builder.Property(cc => cc.TargetChargePercentage).HasColumnType("decimal(5, 2)");
        builder.Property(cc => cc.EnergyConsumed).HasColumnType("decimal(5, 2)").HasDefaultValue(0).IsRequired();
        builder.Property(cc => cc.TotalCost).HasColumnType("decimal(5, 2)").HasDefaultValue(0).IsRequired();
        builder.Property(cc => cc.ToPay).HasDefaultValue(false).IsRequired();
    }
}
