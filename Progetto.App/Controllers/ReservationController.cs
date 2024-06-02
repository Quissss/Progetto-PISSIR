using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

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
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
    {
        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult<Reservation>> DeleteReservation(Reservation reservation)
    {
        await _reservationRepository.DeleteAsync(r => r.Id == reservation.Id);
        await _reservationRepository.SaveAsync();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<Reservation>> UpdateReservation(Reservation reservation)
    {
        await _reservationRepository.UpdateAsync(reservation);
        await _reservationRepository.SaveAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetAllReservations()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        return Ok(reservations);
    }
}
