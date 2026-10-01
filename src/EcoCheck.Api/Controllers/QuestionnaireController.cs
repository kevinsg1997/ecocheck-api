using EcoCheck.Api.Dtos;
using EcoCheck.Api.Infrastructure;
using EcoCheck.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoCheck.Api.Controllers;

[ApiController]
[Route("api/questionnaire")]
[EnableRateLimiting(RateLimitPolicies.Read)]
public class QuestionnaireController(SurveyService surveyService) : ControllerBase
{
    /// <summary>Retorna as perguntas ativas e suas alternativas (sem pontuação).</summary>
    [HttpGet]
    [ProducesResponseType<QuestionnaireDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuestionnaireDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await surveyService.GetQuestionnaireAsync(cancellationToken));
    }
}
