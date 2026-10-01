using EcoCheck.Api.Dtos;
using EcoCheck.Api.Infrastructure;
using EcoCheck.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoCheck.Api.Controllers;

[ApiController]
[Route("api/statistics")]
[EnableRateLimiting(RateLimitPolicies.Read)]
public class StatisticsController(StatisticsService statisticsService) : ControllerBase
{
    /// <summary>Retorna apenas dados agregados de todas as participações.</summary>
    [HttpGet]
    [ProducesResponseType<StatisticsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await statisticsService.GetAsync(cancellationToken));
    }
}
