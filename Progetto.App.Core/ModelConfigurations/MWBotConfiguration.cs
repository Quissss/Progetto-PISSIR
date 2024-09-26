using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

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
        builder.Property(b => b.LatestLocation)
            .HasConversion(
                value => (int)value,
                value => Enum.Parse<MwBotLocations>(value.ToString())
            );
        builder.HasOne(b => b.Parking).WithMany().HasForeignKey(b => b.ParkingId);
    }
}
