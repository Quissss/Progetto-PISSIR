using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ILogger<ReservationController> _logger;
    private readonly ReservationRepository _reservationRepository;

    public ReservationController(ILogger<ReservationController> logger, ReservationRepository repository)
    {
        _logger = logger;
        _reservationRepository = repository;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Reservation>> CreateReservation([FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogDebug("Creating reservation with id {id}", reservation.Id);

            var existingReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
            if (existingReservation != null)
            {
                _logger.LogWarning("Reservation with id {id} already exists", reservation.Id);
                return BadRequest();
            }

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating reservation with id {id}", reservation.Id);
        }

        return BadRequest();

    }

    [HttpDelete]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Reservation>> DeleteReservation([FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogDebug("Deleting reservation with id {id}", reservation.Id);

            var existingReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with id {id} does not exist", reservation.Id);
                return BadRequest();
            }

            await _reservationRepository.DeleteAsync(r => r.Id == reservation.Id);
            await _reservationRepository.SaveAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting reservation with id {id}", reservation.Id);
        }

        return BadRequest();
    }

    [HttpPut]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Reservation>> UpdateReservation([FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogDebug("Updating reservation with id {id}", reservation.Id);

            var existingReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with id {id} does not exist", reservation.Id);
                return BadRequest();
            }

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating reservation with id {id}", reservation.Id);
        }

        return BadRequest();
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetAllReservations()
    {
        try
        {
            _logger.LogDebug("Getting all reservations");

            var reservations = await _reservationRepository.GetAllAsync();
            if (reservations == null)
            {
                _logger.LogWarning("No reservations found");
                return NotFound();
            }

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all reservations");
        }

        return BadRequest();
    }
}
