﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Progetto.App.Core.Data;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Car", b =>
                {
                    b.Property<string>("LicencePlate")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsElectric")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParkingId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParkingSlotId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("LicencePlate");

                    b.HasIndex("OwnerId");

                    b.HasIndex("ParkingId");

                    b.ToTable("Cars", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.CurrentlyCharging", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarPlate")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("CurrentChargePercentage")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<DateTime?>("EndChargingTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("EnergyConsumed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(5, 2)")
                        .HasDefaultValue(0m);

                    b.Property<int?>("ImmediateRequestId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MwBotId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParkingSlotId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("StartChargePercentage")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<DateTime?>("StartChargingTime")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2024, 10, 1, 1, 33, 28, 518, DateTimeKind.Local).AddTicks(9168));

                    b.Property<decimal?>("TargetChargePercentage")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<bool>("ToPay")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<decimal>("TotalCost")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(5, 2)")
                        .HasDefaultValue(0m);

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CarPlate");

                    b.HasIndex("ImmediateRequestId");

                    b.HasIndex("MwBotId");

                    b.HasIndex("ParkingSlotId");

                    b.HasIndex("UserId");

                    b.ToTable("CurrentlyCharging", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.ImmediateRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarLicencePlate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CarPlate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("FromReservation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ParkingId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParkingSlotId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("RequestDate")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("RequestedChargeLevel")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CarLicencePlate");

                    b.HasIndex("ParkingId");

                    b.HasIndex("ParkingSlotId");

                    b.HasIndex("UserId");

                    b.ToTable("ImmediateRequests");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.MwBot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BatteryPercentage")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParkingId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ParkingId");

                    b.ToTable("MWBots", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Parking", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("EnergyCostPerKw")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("StopCostPerMinute")
                        .HasColumnType("decimal(5, 2)");

                    b.HasKey("Id");

                    b.ToTable("Parkings", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.ParkingSlot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ParkingId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ParkingId");

                    b.ToTable("ParkingSlots", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.PaymentHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarPlate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("EndChargePercentage")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("EnergyConsumed")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCharge")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PaymentDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2024, 10, 1, 1, 33, 28, 521, DateTimeKind.Local).AddTicks(8355));

                    b.Property<decimal?>("StartChargePercentage")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TotalCost")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CarPlate");

                    b.HasIndex("UserId");

                    b.ToTable("PaymentHistory", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CarIsInside")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarPlate")
                        .HasColumnType("TEXT");

                    b.Property<int>("ParkingId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("RequestDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("RequestedChargeLevel")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ReservationTime")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CarPlate");

                    b.HasIndex("ParkingId");

                    b.HasIndex("UserId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Stopover", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarPlate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndStopoverTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParkingSlotId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("StartStopoverTime")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue(new DateTime(2024, 10, 1, 1, 33, 28, 523, DateTimeKind.Local).AddTicks(1390));

                    b.Property<bool>("ToPay")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<decimal>("TotalCost")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CarPlate");

                    b.HasIndex("ParkingSlotId");

                    b.HasIndex("UserId");

                    b.ToTable("Stopover", (string)null);
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Users.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("TelegramUsername")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Car", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.Parking", "Parking")
                        .WithMany()
                        .HasForeignKey("ParkingId");

                    b.Navigation("Owner");

                    b.Navigation("Parking");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.CurrentlyCharging", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarPlate");

                    b.HasOne("Progetto.App.Core.Models.ImmediateRequest", "ImmediateRequest")
                        .WithMany()
                        .HasForeignKey("ImmediateRequestId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Progetto.App.Core.Models.MwBot", "MwBot")
                        .WithMany()
                        .HasForeignKey("MwBotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.ParkingSlot", "ParkingSlot")
                        .WithMany()
                        .HasForeignKey("ParkingSlotId");

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Car");

                    b.Navigation("ImmediateRequest");

                    b.Navigation("MwBot");

                    b.Navigation("ParkingSlot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.ImmediateRequest", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarLicencePlate");

                    b.HasOne("Progetto.App.Core.Models.Parking", "Parking")
                        .WithMany()
                        .HasForeignKey("ParkingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.ParkingSlot", "ParkingSlot")
                        .WithMany()
                        .HasForeignKey("ParkingSlotId");

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Parking");

                    b.Navigation("ParkingSlot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.MwBot", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Parking", "Parking")
                        .WithMany()
                        .HasForeignKey("ParkingId");

                    b.Navigation("Parking");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.ParkingSlot", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Parking", "Parking")
                        .WithMany("ParkingSlots")
                        .HasForeignKey("ParkingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parking");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.PaymentHistory", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarPlate")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Reservation", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarPlate");

                    b.HasOne("Progetto.App.Core.Models.Parking", "Parking")
                        .WithMany()
                        .HasForeignKey("ParkingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Parking");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Stopover", b =>
                {
                    b.HasOne("Progetto.App.Core.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarPlate");

                    b.HasOne("Progetto.App.Core.Models.ParkingSlot", "ParkingSlot")
                        .WithMany()
                        .HasForeignKey("ParkingSlotId");

                    b.HasOne("Progetto.App.Core.Models.Users.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Car");

                    b.Navigation("ParkingSlot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Progetto.App.Core.Models.Parking", b =>
                {
                    b.Navigation("ParkingSlots");
                });
#pragma warning restore 612, 618
        }
    }
}
