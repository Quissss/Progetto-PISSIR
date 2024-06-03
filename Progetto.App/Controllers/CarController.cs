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
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Car>> Add([FromBody] Car car)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogDebug("Creating car with licence plate {licencePlate}", car.LicencePlate);

            var existingCar = await _repository.GetCarByLicencePlate(car.LicencePlate);
            if (existingCar != null)
            {
                _logger.LogWarning("Car with licence plate {licencePlate} already exists", car.LicencePlate);
                return BadRequest();
            }

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
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> Delete([FromBody] string licencePlate)
    {
        if (string.IsNullOrEmpty(licencePlate))
            return BadRequest();

        try
        {
            _logger.LogDebug("Deleting car with licence plate {licencePlate}", licencePlate);

            var existingCar = await _repository.GetCarByLicencePlate(licencePlate);
            if (existingCar == null)
            {
                _logger.LogWarning("Reservation with plate {licencePlate} doesn't exist", licencePlate);
                return BadRequest();
            }

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
    [Authorize(Policy = PolicyNames.IsAdmin)]
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
    [Authorize(Policy = PolicyNames.IsAdmin)]
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
        if (string.IsNullOrEmpty(licencePlate))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting car with licence plate {licencePlate}", licencePlate);

            var car = await _repository.GetCarByLicencePlate(licencePlate);
            if (car == null)
                return NotFound();

            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting car with licence plate {licencePlate}", licencePlate);
        }

        return BadRequest();
    }

    [HttpGet("my-cars")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCars()
    {
        if (string.IsNullOrEmpty(User.Identity.Name))
            return BadRequest();
        
        try
        {
            _logger.LogDebug("Getting cars of user {user}", User.Identity.Name);

            var car = await _repository.GetCarsByOwner(User.Identity.Name);
            if (car == null)
                return NotFound();

            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cars of user {user}", User.Identity.Name);
        }

        return BadRequest();
    }

    [HttpGet("my-cars/{licencePlate}")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCar([FromBody] string licencePlate)
    {
        if (string.IsNullOrEmpty(licencePlate))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting car with licence plate {licencePlate} of user {user}", licencePlate, User.Identity.Name);

            var myCars = await _repository.GetCarsByOwner(User.Identity.Name);

            var car = myCars.Where(c => c.LicencePlate == licencePlate).FirstOrDefault();
            if (car == null)
                return NotFound();

            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting car with licence plate {licencePlate} of user {user}", licencePlate, User.Identity.Name);
        }

        return BadRequest();
    }

    [HttpGet("owner/{ownerId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Car>>> GetByOwner([FromBody] string ownerId)
    {
        if (string.IsNullOrEmpty(ownerId))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting cars of user {ownerId}", ownerId);

            var car = await _repository.GetCarsByOwner(ownerId);
            if (car == null)
                return NotFound();

            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cars of user {ownerId}", ownerId);
        }

        return BadRequest();
    }
}
