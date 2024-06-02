using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.ModelConfigurations;

public class ParkingSlotConfiguration : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion(
                value => (int)value,
                value => Enum.Parse<ParkSlotStatus>(value.ToString())
            );
    }
}
