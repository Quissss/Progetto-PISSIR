using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Progetto.App.Core.Models;
using System.Reflection;

namespace Progetto.App.Core.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
        }
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<Car> Cars { get; set; }
    public DbSet<MWBot> MWBots { get; set; }
    public DbSet<Parking> Parkings { get; set; }
    public DbSet<ParkingSlot> ParkingSlots { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ChargeHistory> ChargeHistory { get; set; }

}
