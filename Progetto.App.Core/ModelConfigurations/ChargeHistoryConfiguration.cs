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
public class ChargeHistoryConfiguration : IEntityTypeConfiguration<ChargeHistory>
{
    public void Configure(EntityTypeBuilder<ChargeHistory> builder)
    {
        builder.ToTable("ChargeHistory");
        builder.HasKey(ch => ch.Id);
        builder.Property(ch => ch.StartChargingTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(ch => ch.EndChargingTime).IsRequired();
        builder.HasOne(ch => ch.Car).WithMany().HasForeignKey(ch => ch.CarPlate);
        builder.HasOne(ch => ch.MwBot).WithMany().HasForeignKey(ch => ch.MwBotId);
        builder.HasOne(ch => ch.User).WithMany().HasForeignKey(ch => ch.UserId);
        builder.HasOne(ch => ch.ParkingSlot).WithMany().HasForeignKey(ch => ch.ParkingSlotId);
    }
}
