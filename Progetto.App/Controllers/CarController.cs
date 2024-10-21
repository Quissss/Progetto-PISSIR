using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Users;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Services.SignalR.Hubs;
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
    private readonly IHubContext<CarHub> _hubContext;
    private readonly CarRepository _carRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public CarController(ILogger<CarController> logger, IHubContext<CarHub> hubContext, CarRepository repository, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _hubContext = hubContext;
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
            _logger.LogWarning("Invalid model state while creating car with licence plate {plate}", car.Plate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating car with licence plate {plate}", car.Plate);

            await _carRepository.AddAsync(car);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("CarAdded", car);

            _logger.LogDebug("Car with licence plate {plate} created", car.Plate);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating car with licence plate {plate}", car.Plate);
        }

        return BadRequest();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCar([FromBody] Car car)
    {
        var validator = new CarValidator();
        var result = validator.Validate(car);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating car with licence plate {plate}", car.Plate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting car with licence plate {plate}", car.Plate);

            var existingCar = await _carRepository.GetCarByPlate(licencePlate);
            if (existingCar == null)
            {
                _logger.LogWarning("Reservation with plate {plate} doesn't exist", car.Plate);
                return NotFound();
            }

            await _carRepository.DeleteAsync(c => c.Plate == car.Plate);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("CarDeleted", car.Plate);

            _logger.LogDebug("Car with licence plate {plate} deleted", car.Plate);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting car with licence plate {plate}", car.Plate);
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
            _logger.LogWarning("Invalid model state while updating car with licence plate {plate}", car.Plate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating car with licence plate {plate}", car.Plate);

            if (!(await _carRepository.CheckEntityExists(car)))
            {
                _logger.LogWarning("Car with licence plate {plate} not found", car.Plate);
                return NotFound(car);
            }

            await _carRepository.UpdateAsync(car);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("CarUpdated", car);

            _logger.LogDebug("Updated car with values: {car}", car);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating car with licence plate {plate}", car.Plate);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Car>>> GetAllCars([FromQuery] string? plate, [FromQuery] CarStatus? status)
    {
        try
        {
            _logger.LogDebug("Getting all cars");
            var cars = await _carRepository.GetAllAsync();
            if (cars == null)
            {
                _logger.LogWarning("No cars found");
                return NotFound();
            }

            if (!string.IsNullOrEmpty(plate))
                cars = cars.Where(car => car.Plate.Contains(plate)).ToList();
            if (status.HasValue) 
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

    [HttpGet("{plate}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Car>> GetCarByLicencePlate(string plate)
    {
        if (string.IsNullOrEmpty(plate))
        {
            _logger.LogWarning("Invalid licence plate {plate}", plate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting car with licence plate {plate}", plate);

            var car = await _carRepository.GetCarByPlate(licencePlate);
            if (car == null)
            {
                _logger.LogWarning("Car with licence plate {plate} not found", plate);
                return NotFound();
            }

            _logger.LogDebug("Returning car with licence plate {plate}", plate);
            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting car with licence plate {plate}", plate);
        }

        return BadRequest();
    }

    [HttpGet("owner/{ownerId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Car>>> GetCarsByOwner(string ownerId, [FromQuery] string? plate, [FromQuery] CarStatus? status)
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

            if (!string.IsNullOrEmpty(plate))
                cars = cars.Where(car => car.Plate.Contains(plate)).ToList();
            if (status.HasValue)
                cars = cars.Where(car => car.Status == status).ToList();

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
    public async Task<ActionResult<IEnumerable<Car>>> GetMyCars([FromQuery] string? plate, [FromQuery] CarStatus? status)
    {
        var userId = (await _userManager.GetUserAsync(User))?.Id.ToString();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid user {user}", userId);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting cars of user {user}", userId);

            var cars = await _carRepository.GetCarsByOwner(userId);
            if (cars == null)
            {
                _logger.LogWarning("No cars found for user {user}", userId);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(plate))
                cars = cars.Where(car => car.Plate.Contains(plate)).ToList();
            if (status.HasValue)
                cars = cars.Where(car => car.Status == status).ToList();

            _logger.LogDebug("Returning cars of user {user}", userId);
            return Ok(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cars of user {user}", userId);
        }

        return BadRequest();
    }
}
