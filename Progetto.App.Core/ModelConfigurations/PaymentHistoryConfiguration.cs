﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.ModelConfigurations;

/// <summary>
/// Configuration for the payment history in the database (EF Core)
/// </summary>
public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
        builder.ToTable("PaymentHistory");
        builder.HasKey(so => so.Id);
        builder.Property(so => so.PaymentDate).HasDefaultValue(DateTime.Now).IsRequired();
        builder.Property(so => so.StartTime).IsRequired();
        builder.Property(so => so.EndTime).IsRequired();
        builder.Property(so => so.StartChargePercentage).IsRequired(false);
        builder.Property(so => so.EndChargePercentage).IsRequired(false);
        builder.HasOne(so => so.Car).WithMany().HasForeignKey(so => so.CarPlate).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(so => so.User).WithMany().HasForeignKey(so => so.UserId);
        builder.Property(so => so.TotalCost).HasColumnType("decimal(5, 2)");
    }
}
