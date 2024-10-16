using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.SignalR.Hubs;

public class CarHub : Hub
{
    private readonly ILogger<CarHub> _logger;

    public CarHub(ILogger<CarHub> logger)
    {
        _logger = logger;
    }

    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("New client connected");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected with {exception}", exception != null ? $"with {exception.Message}" : "no exception");
        return base.OnDisconnectedAsync(exception);
    }
}
