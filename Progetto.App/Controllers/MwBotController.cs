using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MwBotController : ControllerBase
{
    private readonly ILogger<MwBotController> _logger;
    private readonly MwBotRepository _repository;
    private readonly ChargeHistoryRepository _chargeHistoryRepository;

    public MwBotController(ILogger<MwBotController> logger, MwBotRepository repository, ChargeHistoryRepository chargeHistoryRepository)
    {
        _logger = logger;
        _repository = repository;
        _chargeHistoryRepository = chargeHistoryRepository;
    }
}
