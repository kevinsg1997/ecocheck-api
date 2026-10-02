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
    /// <summary>
    /// Retorna apenas dados agregados. Filtro opcional por região:
    /// <c>?country=BR</c> ou <c>?country=BR&amp;state=SP</c>.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<StatisticsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] string? country,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        var countryCode = RegionCatalog.Normalize(country);
        var stateCode = RegionCatalog.Normalize(state);

        var errors = new Dictionary<string, string[]>();
        RegionCatalog.Validate(countryCode, stateCode, errors, countryKey: "country", stateKey: "state");
        if (errors.Count > 0)
        {
            foreach (var (key, messages) in errors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(key, message);
                }
            }

            return ValidationProblem();
        }

        var filter = new StatisticsFilter(countryCode, stateCode);
        return Ok(await statisticsService.GetAsync(filter, cancellationToken));
    }
}
