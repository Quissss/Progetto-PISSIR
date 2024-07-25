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
public class StopoverHistoryConfiguration : IEntityTypeConfiguration<StopoverHistory>
{
    public void Configure(EntityTypeBuilder<StopoverHistory> builder)
    {
        builder.ToTable("StopoverHistory");
        builder.HasKey(so => so.Id);
        builder.Property(so => so.StartStopoverTime).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(so => so.EndStopoverTime).IsRequired();
        builder.HasOne(so => so.Car).WithMany().HasForeignKey(so => so.CarPlate);
        builder.HasOne(so => so.User).WithMany().HasForeignKey(so => so.UserId);
        builder.Property(so => so.TotalCost).HasColumnType("decimal(5, 2)");
    }
}
