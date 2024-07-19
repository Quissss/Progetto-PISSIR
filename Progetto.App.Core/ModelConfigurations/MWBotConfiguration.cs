﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the MwBot entity (table) in the database (EF Core)
/// </summary>
public class MwBotConfiguration : IEntityTypeConfiguration<MwBot>
{
    public void Configure(EntityTypeBuilder<MwBot> builder)
    {
        builder.ToTable("MWBots");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BatteryPercentage).IsRequired();
        builder.Property(b => b.Status).IsRequired()
            .HasConversion(
                value => (int)value,
                value => Enum.Parse<MwBotStatus>(value.ToString())
            );
        builder.HasOne(b => b.Parking).WithMany().HasForeignKey(b => b.ParkingId);
    }
}
