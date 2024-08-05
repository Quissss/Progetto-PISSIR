using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayPal.REST.Client;
using PayPal.REST.Models;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Security.Policies;
using Progetto.App.Core.Services.Mqtt;
using Progetto.App.Core.Validators;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(
        dispose: true,
        logger: new LoggerConfiguration()
            .WriteTo.File(@$"Logs\{DateTime.Now:yyyyMMdd-HHmm}.log")
            .CreateLogger()
    ));

#region Google Authentication
services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
    });
#endregion

#region PayPal
services.AddSingleton<IPayPalClient, PayPalClient>();
services.Configure<PayPalClientOptions>(options =>
{
    options.ClientId = configuration.GetRequiredSection("PayPal")["ClientId"]!;
    options.ClientSecret = configuration.GetRequiredSection("PayPal")["ClientSecret"]!;
    options.PayPalUrl = configuration.GetRequiredSection("PayPal")["PayPalUrl"]!;
});
#endregion

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
services.AddDbContext<ApplicationDbContext>();
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
services.AddRazorPages();

services.AddAuthorization(option =>
{
    option.AddPolicy(PolicyNames.IsAdmin, policy => policy.AddRequirements(new IsAdmin()));
    option.AddPolicy(PolicyNames.IsPremiumUser, policy => policy.AddRequirements(new IsPremiumUser()));
});

// Validators
services.AddValidatorsFromAssemblyContaining<CarValidator>();

// Repositories
services.AddScoped<CarRepository>();
services.AddScoped<MwBotRepository>();
services.AddScoped<ParkingRepository>();
services.AddScoped<ParkingSlotRepository>();
services.AddScoped<ReservationRepository>();
services.AddScoped<ImmediateRequestRepository>();
services.AddScoped<CurrentlyChargingRepository>();
services.AddScoped<StopoverRepository>();
services.AddScoped<PaymentHistoryRepository>();

// Authorization handlers
services.AddSingleton<ChargeManager>();
services.AddSingleton<IAuthorizationHandler, IsAdminAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, IsPremiumUserAuthorizationHandler>();

// Mqtt
services.AddHostedService<MqttBroker>();
services.AddTransient<MqttMwBotClient>();

services.AddSingleton<ConnectedClientsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

// Ensure the MQTT broker is started before initializing clients
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var connectedClientsService = scope.ServiceProvider.GetRequiredService<ConnectedClientsService>();
        await connectedClientsService.InitializeConnectedClients();
    }
});

app.Run();
