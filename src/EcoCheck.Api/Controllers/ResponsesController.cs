using EcoCheck.Api.Dtos;
using EcoCheck.Api.Infrastructure;
using EcoCheck.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoCheck.Api.Controllers;

[ApiController]
[Route("api/responses")]
public class ResponsesController(SurveyService surveyService, StatisticsService statisticsService) : ControllerBase
{
    private const long MaxRequestBodyBytes = 16 * 1024;

    /// <summary>Recebe as respostas anônimas, calcula e salva o resultado.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Submit)]
    [RequestSizeLimit(MaxRequestBodyBytes)]
    [ProducesResponseType<SurveyResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SurveyResultDto>> Submit(
        SubmitResponseRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await surveyService.SubmitAsync(request, cancellationToken);

        if (!outcome.IsValid)
        {
            foreach (var (key, messages) in outcome.Errors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(key, message);
                }
            }

            return ValidationProblem();
        }

        statisticsService.Invalidate();
        return Ok(outcome.Result);
    }
}
