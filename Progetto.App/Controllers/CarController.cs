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
/// Controller per la gestione delle operazioni sulle auto (endpoints CRUD).
/// Richiede l'autenticazione.
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

    /// <summary>
    /// Inizializza un'istanza del CarController.
    /// </summary>
    /// <param name="logger">Logger utilizzato per il logging delle operazioni.</param>
    /// <param name="hubContext">Hub SignalR per la comunicazione in tempo reale.</param>
    /// <param name="repository">Repository per la gestione delle operazioni sui dati delle auto.</param>
    /// <param name="userManager">Gestore delle operazioni sugli utenti.</param>
    public CarController(ILogger<CarController> logger, IHubContext<CarHub> hubContext, CarRepository repository, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _hubContext = hubContext;
        _carRepository = repository;
        _userManager = userManager;
    }

    /// <summary>
    /// Aggiunge una nuova auto.
    /// </summary>
    /// <param name="car">Oggetto auto che deve essere creato.</param>
    /// <returns>Restituisce l'oggetto auto creato o un messaggio di errore.</returns>
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

    /// <summary>
    /// Elimina un'auto esistente.
    /// </summary>
    /// <param name="car">Oggetto auto da eliminare in base alla targa.</param>
    /// <returns>Restituisce l'auto eliminata o un messaggio di errore.</returns>
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

            var existingCar = await _carRepository.GetCarByPlate(car.Plate);
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

    /// <summary>
    /// Aggiorna le informazioni di un'auto esistente.
    /// </summary>
    /// <param name="car">Oggetto auto con le nuove informazioni da aggiornare.</param>
    /// <returns>Restituisce l'oggetto auto aggiornato o un messaggio di errore.</returns>
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

    /// <summary>
    /// Recupera tutte le auto, con eventuali filtri basati su targa e stato.
    /// </summary>
    /// <param name="plate">Filtro opzionale per la targa dell'auto.</param>
    /// <param name="status">Filtro opzionale per lo stato dell'auto.</param>
    /// <returns>Restituisce la lista di auto o un messaggio di errore.</returns>
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

    /// <summary>
    /// Recupera i dettagli di un'auto tramite la targa.
    /// </summary>
    /// <param name="plate">Targa dell'auto da recuperare.</param>
    /// <returns>Restituisce l'auto trovata o un messaggio di errore.</returns>
    [HttpGet("{plate}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Car>> GetCarByPlate(string plate)
    {
        if (string.IsNullOrEmpty(plate))
        {
            _logger.LogWarning("Invalid licence plate {plate}", plate);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting car with licence plate {plate}", plate);

            var car = await _carRepository.GetCarByPlate(plate);
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

    /// <summary>
    /// Recupera tutte le auto di un proprietario specifico, con eventuali filtri.
    /// </summary>
    /// <param name="ownerId">ID del proprietario delle auto.</param>
    /// <param name="plate">Filtro opzionale per la targa.</param>
    /// <param name="status">Filtro opzionale per lo stato dell'auto.</param>
    /// <returns>Restituisce la lista di auto del proprietario o un messaggio di errore.</returns>
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

    /// <summary>
    /// Recupera tutte le auto dell'utente autenticato.
    /// </summary>
    /// <param name="plate">Filtro opzionale per la targa.</param>
    /// <param name="status">Filtro opzionale per lo stato dell'auto.</param>
    /// <returns>Restituisce la lista di auto dell'utente autenticato o un messaggio di errore.</returns>
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
