using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the ChargeHistory entity (table) in the database (EF Core)
/// </summary>
public class CurrentlyChargingConfiguration : IEntityTypeConfiguration<CurrentlyCharging>
{
    public void Configure(EntityTypeBuilder<CurrentlyCharging> builder)
    {
        builder.ToTable("CurrentlyCharging");
        builder.HasKey(cc => cc.Id);
        builder.Property(cc => cc.StartChargingTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.HasOne(cc => cc.Car).WithMany().HasForeignKey(cc => cc.CarPlate);
        builder.HasOne(cc => cc.MwBot).WithMany().HasForeignKey(cc => cc.MwBotId);
        builder.HasOne(cc => cc.User).WithMany().HasForeignKey(cc => cc.UserId);
        builder.HasOne(cc => cc.ParkingSlot).WithMany().HasForeignKey(cc => cc.ParkingSlotId);
    }
}
