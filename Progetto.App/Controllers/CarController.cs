using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Security.Policies;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CarController : ControllerBase
{
    private readonly ILogger<CarController> _logger;
    private readonly CarRepository _repository;

    public CarController(ILogger<CarController> logger, CarRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpPost]
    public async Task<ActionResult<Car>> Add([FromBody] Car car)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogDebug("Creating car with licence plate {licencePlate}", car.LicencePlate);

            await _repository.AddAsync(car);
            await _repository.SaveAsync();

            _logger.LogDebug("Car with {licencePlate} created", car.LicencePlate);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating car with licence plate {licencePlate}", car.LicencePlate);
        }

        return BadRequest();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] string licencePlate)
    {
        try
        {
            _logger.LogDebug("Deleting car with licence plate {licencePlate}", licencePlate);

            var car = await _repository.GetCarByLicencePlate(licencePlate);
            if (car == null)
                return NotFound();

            await _repository.DeleteAsync(c => c.LicencePlate == licencePlate && c.OwnerId == User.Identity.Name);
            await _repository.SaveAsync();

            _logger.LogDebug("Car with licence plate {licencePlate} deleted", licencePlate);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting car with licence plate {licencePlate}", licencePlate);
        }

        return BadRequest();
    }

    [HttpPut]
    public async Task<ActionResult<Car>> Update([FromBody] Car car)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (car.Model == null || car.Brand == null || car.LicencePlate == null)
                return BadRequest();

            var licencePlate = car.LicencePlate;
            _logger.LogDebug("Updating car with licence plate {licencePlate}", licencePlate);

            var foundCar = await _repository.GetCarByLicencePlate(licencePlate);
            if (foundCar == null)
                return NotFound();

            _logger.LogDebug("Updated car with values: {car}", car);

            await _repository.UpdateAsync(car);
            await _repository.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating car with licence plate {licencePlate}", car.LicencePlate);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Car>>> GetAll()
    {
        try
        {
            _logger.LogDebug("Getting all cars");

            var cars = await _repository.GetAllAsync();
            if (cars.Count() == 0)
                return NotFound();

            _logger.LogDebug("Got all cars");
            return Ok(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all cars");
        }

        return BadRequest();
    }

    [HttpGet("{licencePlate}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Car>> GetByLicencePlate([FromBody] string licencePlate)
    {
        var car = await _repository.GetCarByLicencePlate(licencePlate);
        if (car == null)
            return NotFound();

        return Ok(car);
    }

    [HttpGet("my-cars")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCars()
    {
        var car = await _repository.GetCarsByOwner(User.Identity.Name);
        if (car == null)
            return NotFound();

        return Ok(car);
    }

    [HttpGet("my-cars/{licencePlate}")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCar([FromBody] string licencePlate)
    {
        var myCars = await _repository.GetCarsByOwner(User.Identity.Name);

        var car = myCars.Where(c => c.LicencePlate == licencePlate).FirstOrDefault();
        if (car == null)
            return NotFound();

        return Ok(car);
    }

    [HttpGet("owner/{ownerId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Car>>> GetByOwner([FromBody] string ownerId)
    {
        var car = await _repository.GetCarsByOwner(ownerId);
        if (car == null)
            return NotFound();

        return Ok(car);
    }
}
