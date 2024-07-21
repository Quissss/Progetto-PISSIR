using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Validators;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for car operations (endpoints for CRUD operations)
/// Requires authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CarController : ControllerBase
{
    private readonly ILogger<CarController> _logger;
    private readonly CarRepository _carRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public CarController(ILogger<CarController> logger, CarRepository repository, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _carRepository = repository;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<Car>> AddCar([FromBody] Car car)
    {
        var validator = new CarValidator();
        var result = validator.Validate(car);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating car with licence plate {licencePlate}", car.LicencePlate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating car with licence plate {licencePlate}", car.LicencePlate);

            await _carRepository.AddAsync(car);

            _logger.LogDebug("Car with licence plate {licencePlate} created", car.LicencePlate);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating car with licence plate {licencePlate}", car.LicencePlate);
        }

        return BadRequest();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCar([FromBody] string licencePlate)
    {
        if (string.IsNullOrEmpty(licencePlate))
        {
            _logger.LogWarning("Invalid licence plate {licencePlate}", licencePlate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting car with licence plate {licencePlate}", licencePlate);

            var existingCar = await _carRepository.GetCarByLicencePlate(licencePlate);
            if (existingCar == null)
            {
                _logger.LogWarning("Reservation with plate {licencePlate} doesn't exist", licencePlate);
                return NotFound();
            }

            await _carRepository.DeleteAsync(c => c.LicencePlate == licencePlate);

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
    public async Task<ActionResult<Car>> UpdateCar([FromBody] Car car)
    {
        var validator = new CarValidator();
        var result = validator.Validate(car);
        result.AddToModelState(ModelState);


        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating car with licence plate {licencePlate}", car.LicencePlate);
            return BadRequest();
        }

        try
        {
            var licencePlate = car.LicencePlate;
            _logger.LogDebug("Updating car with licence plate {licencePlate}", licencePlate);

            var existingCar = await _carRepository.GetCarByLicencePlate(licencePlate);
            if (existingCar == null)
            {
                _logger.LogWarning("Car with licence plate {licencePlate} not found", licencePlate);
                return NotFound();
            }

            await _carRepository.UpdateAsync(car);

            _logger.LogDebug("Updated car with values: {car}", car);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating car with licence plate {licencePlate}", car.LicencePlate);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Car>>> GetAllCars([FromQuery] string? licencePlate,  [FromQuery] CarStatus? status)
    {
        try
        {
            _logger.LogDebug("Getting all cars");
            var user = await _userManager.GetUserAsync(User);
            var cars = await _carRepository.GetCarsByOwner(user.Id);

            if (!string.IsNullOrEmpty(licencePlate))
                cars=cars.Where(car => car.LicencePlate.Contains(licencePlate)).ToList();
            if (status is not null)
                cars = cars.Where(car => car.Status == status).ToList();

            _logger.LogDebug("Returning {count} cars", cars.Count());

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
    public async Task<ActionResult<Car>> GetCarByLicencePlate(string licencePlate)
    {
        if (string.IsNullOrEmpty(licencePlate))
        {
            _logger.LogWarning("Invalid licence plate {licencePlate}", licencePlate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting car with licence plate {licencePlate}", licencePlate);

            var car = await _carRepository.GetCarByLicencePlate(licencePlate);
            if (car == null)
            {
                _logger.LogWarning("Car with licence plate {licencePlate} not found", licencePlate);
                return NotFound();
            }

            _logger.LogDebug("Returning car with licence plate {licencePlate}", licencePlate);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting car with licence plate {licencePlate}", licencePlate);
        }

        return BadRequest();
    }

    [HttpGet("owner/{ownerId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Car>>> GetCarsByOwner(string ownerId)
    {
        if (string.IsNullOrEmpty(ownerId))
        {
            _logger.LogWarning("Invalid owner id {ownerId}", ownerId);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting cars of user {ownerId}", ownerId);

            var cars = await _carRepository.GetCarsByOwner(ownerId);
            if (cars == null)
            {
                _logger.LogWarning("No cars found for user {ownerId}", ownerId);
                return NotFound();
            }

            _logger.LogDebug("Returning cars of user {ownerId}", ownerId);
            return Ok(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cars of user {ownerId}", ownerId);
        }

        return BadRequest();
    }

    [HttpGet("my-cars")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCars()
    {
        if (string.IsNullOrEmpty(User.Identity.Name))
        {
            _logger.LogWarning("Invalid user {user}", User.Identity.Name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting cars of user {user}", User.Identity.Name);

            var car = await _carRepository.GetCarsByOwner(User.Identity.Name);
            if (car == null)
            {
                _logger.LogWarning("No cars found for user {user}", User.Identity.Name);
                return NotFound();
            }

            _logger.LogDebug("Returning cars of user {user}", User.Identity.Name);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cars of user {user}", User.Identity.Name);
        }

        return BadRequest();
    }

    [HttpGet("my-cars/{licencePlate}")]
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCar(string licencePlate)
    {
        if (string.IsNullOrEmpty(licencePlate))
        {
            _logger.LogWarning("Invalid licence plate {licencePlate}", licencePlate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting car with licence plate {licencePlate} of user {user}", licencePlate, User.Identity.Name);

            var myCars = await _carRepository.GetCarsByOwner(User.Identity.Name);

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
}
