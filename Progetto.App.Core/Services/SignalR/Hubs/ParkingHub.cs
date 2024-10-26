using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.SignalR.Hubs;

/// <summary>
/// SignalR hub for managing client connections related to parking services.
/// </summary>
public class ParkingHub : Hub
{
    private readonly ILogger<ParkingHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParkingHub"/> class with a logger instance.
    /// </summary>
    /// <param name="logger">Logger instance for logging connection events.</param>
    public ParkingHub(ILogger<ParkingHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the connection ID of the connected client.
    /// </summary>
    /// <returns>The connection ID of the client.</returns>
    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    /// <summary>
    /// Called when a client connects to the hub. Logs the connection event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("New client connected");
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub. Logs the disconnection event, including any exception if present.
    /// </summary>
    /// <param name="exception">Optional exception that caused the disconnection, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected with {exception}", exception != null ? $"with {exception.Message}" : "no exception");
        return base.OnDisconnectedAsync(exception);
    }
}