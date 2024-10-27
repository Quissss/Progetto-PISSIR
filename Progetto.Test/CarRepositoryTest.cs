using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Progetto.Test
{
    public class CarRepositoryTests
    {
        private ApplicationDbContext _context;
        private CarRepository _carRepository;

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
            _context.Cars.AddRange(new List<Car>
            {
                new Car { Plate = "AB123CD", OwnerId = "Owner1", Status = CarStatus.OutOfParking, Brand = "Brand", Model = "Model" },
                new Car { Plate = "DE456FG", OwnerId = "Owner2", Status = CarStatus.InCharge, Brand = "Brand", Model = "Model" },
                new Car { Plate = "GH789IJ", OwnerId = "Owner1", Status = CarStatus.Charged, Brand = "Brand", Model = "Model" },
            });

            _context.SaveChanges();
            _carRepository = new CarRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            // Pulisci il database dopo ogni test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetCarByPlate_ShouldReturnCorrectCar()
        {
            var car = await _carRepository.GetCarByPlate("AB123CD");

            Assert.That(car, Is.Not.Null);
            Assert.That(car.Plate, Is.EqualTo("AB123CD"));
            Assert.That(car.OwnerId, Is.EqualTo("Owner1"));
        }

        [Test]
        public async Task UpdateCarStatus_ShouldUpdateStatusCorrectly()
        {
            var car = await _carRepository.UpdateCarStatus("DE456FG", CarStatus.Charged);

            Assert.That(car, Is.Not.Null);
            Assert.That(car.Status, Is.EqualTo(CarStatus.Charged));
            Assert.That(car.ParkingSlotId, Is.Null);
        }

        [Test]
        public async Task UpdateCarStatus_ShouldSetParkingSlotId_WhenStatusIsInCharge()
        {
            var car = await _carRepository.UpdateCarStatus("AB123CD", CarStatus.InCharge, 5);

            Assert.That(car, Is.Not.Null);
            Assert.That(car.Status, Is.EqualTo(CarStatus.InCharge));
            Assert.That(car.ParkingSlotId, Is.EqualTo(5));
        }
    }
}
