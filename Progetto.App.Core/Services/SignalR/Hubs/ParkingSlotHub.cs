using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.SignalR.Hubs;

/// <summary>
/// SignalR hub for managing connections to the parking slot service.
/// </summary>
public class ParkingSlotHub : Hub
{
    private readonly ILogger<ParkingSlotHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParkingSlotHub"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger used to log connection events.</param>
    public ParkingSlotHub(ILogger<ParkingSlotHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the connection ID of the connected client.
    /// </summary>
    /// <returns>The client's connection ID as a string.</returns>
    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    /// <summary>
    /// Handles the event when a client connects to the hub. Logs the connection event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("New client connected");
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles the event when a client disconnects from the hub. Logs the disconnection event, with details if an exception caused the disconnect.
    /// </summary>
    /// <param name="exception">The exception, if any, that triggered the disconnection.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected with {exception}", exception != null ? $"with {exception.Message}" : "no exception");
        return base.OnDisconnectedAsync(exception);
    }
}