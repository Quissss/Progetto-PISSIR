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
/// Configuration for the Stopover entity (table) in the database (EF Core)
/// </summary>
public class StopoverConfiguration : IEntityTypeConfiguration<Stopover>
{
    public void Configure(EntityTypeBuilder<Stopover> builder)
    {
        builder.ToTable("Stopover");
        builder.HasKey(so => so.Id);
        builder.Property(so => so.StartStopoverTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(so => so.EndStopoverTime).IsRequired(false);
        builder.HasOne(so => so.Car).WithMany().HasForeignKey(so => so.CarPlate);
        builder.HasOne(so => so.User).WithMany().HasForeignKey(so => so.UserId);
        builder.HasOne(so => so.ParkingSlot).WithMany().HasForeignKey(so => so.ParkingSlotId);
        builder.Property(so => so.TotalCost).HasColumnType("decimal(5, 2)");
        builder.Property(so => so.ToPay).HasDefaultValue(false).IsRequired();
    }
}
