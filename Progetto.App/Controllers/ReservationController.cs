﻿using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Validators;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for reservation operations (endpoints for CRUD operations)
/// Requires authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ILogger<ReservationController> _logger;
    private readonly ReservationRepository _reservationRepository;
    private readonly ChargeManager _chargeManager;

    public ReservationController(ILogger<ReservationController> logger, ReservationRepository repository, ChargeManager chargeManager)
    {
        _logger = logger;
        _reservationRepository = repository;
        _chargeManager = chargeManager;
    }


    [HttpPost]
    [Authorize(Policy = PolicyNames.IsPremiumUser)]
    public async Task<ActionResult<Reservation>> CreateReservationForUser([FromForm] Reservation reservation)
    {
        var validator = new ReservationValidator();
        var result = validator.Validate(reservation);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating reservation for user {userId}", reservation.UserId);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating reservation with id {id} for user {user}", reservation.Id, User.Identity.Name);




            reservation.ReservationTime = DateTime.Now;
            await _reservationRepository.AddAsync(reservation);
            _chargeManager.AddReservation(reservation);

            _logger.LogDebug("Reservation created with {id}", reservation.Id);
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
    public async Task<ActionResult<Reservation>> DeleteReservation(int id)
    {
        try
        {
            _logger.LogDebug("Deleting reservation with id {id}", id);

            var existingReservation = await _reservationRepository.GetByIdAsync(id);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with id {id} does not exist", id);
                return NotFound();
            }

            await _reservationRepository.DeleteAsync(r => r.Id == id);

            _logger.LogDebug("Reservation with id {id} deleted", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting reservation with id {id}", id);
        }

        return BadRequest();
    }

    [HttpPut]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Reservation>> UpdateReservation([FromBody] Reservation reservation)
    {
        var validator = new ReservationValidator();
        var result = validator.Validate(reservation);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating reservation with id {id}", reservation.Id);
            return BadRequest();
        }

        try
        {
            var id = reservation.Id;
            _logger.LogDebug("Updating reservation with id {id}", id);

            var existingReservation = await _reservationRepository.GetByIdAsync(id);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with id {id} not found", id);
                return NotFound();
            }

            await _reservationRepository.UpdateAsync(reservation);

            _logger.LogDebug("Reservation updated with values: {reservation}", reservation);
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

            _logger.LogDebug("Returning {count} reservations", reservations.Count());
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all reservations");
        }

        return BadRequest();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Reservation>> GetReservationById(int id)
    {
        if (id <= 0)
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting reservation with id {id}", id);

            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with id {id} not found", id);
                return NotFound();
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting reservation with id {id}", id);
        }

        return BadRequest();
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservationsByUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting reservations of user {userId}", userId);

            var reservations = await _reservationRepository.GetByUserId(userId);
            if (reservations == null)
            {
                _logger.LogWarning("No reservations found for user {userId}", userId);
                return NotFound();
            }

            _logger.LogDebug("Returning reservations of user {userId}", userId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting reservations of user {userId}", userId);
        }

        return BadRequest();
    }

    [HttpGet("my-reservations")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetMyReservations()
    {
        if (string.IsNullOrEmpty(User.Identity.Name))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting reservations of user {user}", User.Identity.Name);

            var reservations = await _reservationRepository.GetByUserId(User.Identity.Name);
            if (reservations == null)
            {
                _logger.LogWarning("No reservations found for user {user}", User.Identity.Name);
                return NotFound();
            }

            _logger.LogDebug("Returning reservations of user {user}", User.Identity.Name);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting my reservations");
        }

        return BadRequest();
    }

    [HttpGet("my-reservation/{id}")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetMyReservation(int id)
    {
        if (string.IsNullOrEmpty(User.Identity.Name))
            return BadRequest();

        try
        {
            _logger.LogDebug("Getting reservation of user {user} with id {id}", User.Identity.Name, id);

            var reservations = await _reservationRepository.GetByUserId(User.Identity.Name);
            if (reservations == null)
            {
                _logger.LogWarning("No reservations found for user {user} with id {id}", User.Identity.Name, id);
                return NotFound();
            }

            _logger.LogDebug("Returning reservation of user {user} with id {id}", User.Identity.Name, id);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting my reservation with id {id}", id);
        }

        return BadRequest();
    }
}
