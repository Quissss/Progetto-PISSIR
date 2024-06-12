using Microsoft.EntityFrameworkCore;
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
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ChargeHistory> builder)
    {
        builder.HasKey(ch => ch.Id);
        builder.Property(ch => ch.ParkStartDate).IsRequired();
        builder.Property(ch => ch.ParkEndDate).IsRequired();
        builder.Property(ch => ch.StartChargeLevel).IsRequired();
        builder.Property(ch => ch.EndChargeLevel).IsRequired();
        builder.HasOne(ch => ch.MWBot).WithMany().HasForeignKey(ch => ch.MWBotId);
        builder.HasOne(ch => ch.User).WithMany().HasForeignKey(ch => ch.UserId);
    }
}
