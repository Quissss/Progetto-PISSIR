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
/// Configuration for the StopOver entity (table) in the database (EF Core)
/// </summary>
public class StopOverConfiguration : IEntityTypeConfiguration<StopOver>
{
    public void Configure(EntityTypeBuilder<StopOver> builder)
    {
        builder.ToTable("StopOver");
        builder.HasKey(so => so.Id);
        builder.Property(so => so.StartStopOverTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(so => so.EndStopoverTime).IsRequired(false);
        builder.HasOne(so => so.Car).WithMany().HasForeignKey(so => so.CarPlate);
        builder.HasOne(so => so.User).WithMany().HasForeignKey(so => so.UserId);
        builder.HasOne(so => so.Parking).WithMany().HasForeignKey(so => so.ParkingId);
        builder.Property(so => so.TotalCost).HasColumnType("decimal(5, 2)");
        builder.Property(so => so.ToPay).HasDefaultValue(false).IsRequired();
    }
}
