using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Progetto.Test
{
    public class MwBotRepositoryTests
    {
        private ApplicationDbContext _context;
        private MwBotRepository _mwBotRepository;

        [SetUp]
        public void Setup()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"ConnectionStrings:DefaultConnection", "DataSource=:memory:"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options, configuration);

            // Popola il database con dati di test
            _context.MwBots.AddRange(new List<MwBot>
            {
                new MwBot { Id = 1, BatteryPercentage = 75.0m, Status = MwBotStatus.StandBy, ParkingId = null, LatestLocation = MwBotLocations.InDock },
                new MwBot { Id = 2, BatteryPercentage = 50.0m, Status = MwBotStatus.ChargingCar, ParkingId = 1, LatestLocation = MwBotLocations.InSlot },
                new MwBot { Id = 3, BatteryPercentage = 20.0m, Status = MwBotStatus.Offline, ParkingId = null, LatestLocation = MwBotLocations.InDock },
            });

            _context.SaveChanges();
            _mwBotRepository = new MwBotRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateMwBot()
        {
            // Arrange
            var mwBot = await _context.MwBots.FindAsync(1);
            mwBot.BatteryPercentage = 80.0m;
            mwBot.Status = MwBotStatus.MovingToDock;

            // Act
            await _mwBotRepository.UpdateAsync(mwBot);

            // Assert
            var updatedMwBot = await _context.MwBots.FindAsync(1);
            Assert.That(updatedMwBot.BatteryPercentage, Is.EqualTo(80.0m));
            Assert.That(updatedMwBot.Status, Is.EqualTo(MwBotStatus.MovingToDock));
        }

        [Test]
        public async Task UpdateMwBotStatus_ShouldUpdateStatus()
        {
            // Act
            var mwBot = await _mwBotRepository.UpdateMwBotStatus(2, MwBotStatus.Recharging);

            // Assert
            Assert.That(mwBot, Is.Not.Null);
            Assert.That(mwBot.Status, Is.EqualTo(MwBotStatus.Recharging));

            // Verifica che lo stato sia aggiornato nel database
            var updatedMwBot = await _context.MwBots.FindAsync(2);
            Assert.That(updatedMwBot.Status, Is.EqualTo(MwBotStatus.Recharging));
        }

        [Test]
        public async Task GetOnlineMwBots_ShouldReturnOnlyOnlineMwBots()
        {
            // Act
            var onlineMwBots = await _mwBotRepository.GetOnlineMwBots();

            // Assert
            Assert.That(onlineMwBots, Is.Not.Null);
            Assert.That(onlineMwBots.Count(), Is.EqualTo(2));
            Assert.That(onlineMwBots.All(m => m.Status != MwBotStatus.Offline), Is.True);
        }

        [Test]
        public async Task GetOfflineMwBots_ShouldReturnOnlyOfflineMwBots()
        {
            // Act
            var offlineMwBots = await _mwBotRepository.GetOfflineMwBots();

            // Assert
            Assert.That(offlineMwBots, Is.Not.Null);
            Assert.That(offlineMwBots.Count(), Is.EqualTo(1));
            Assert.That(offlineMwBots.All(m => m.Status == MwBotStatus.Offline), Is.True);
        }

    }
}
