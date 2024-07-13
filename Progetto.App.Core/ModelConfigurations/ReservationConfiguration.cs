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
/// Configuration for the Reservation entity (table) in the database (EF Core)
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ReservationTime).IsRequired();
        builder.Property(r => r.RequestDate).IsRequired();
        builder.Property(r => r.RequestedChargeLevel).IsRequired();
        builder.HasOne(r => r.ParkingSlot).WithMany().HasForeignKey(r => r.ParkingSlotId);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
    }
}
