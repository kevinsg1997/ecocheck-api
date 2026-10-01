using EcoCheck.Api.Dtos;

namespace EcoCheck.Api.Services;

public record SubmissionOutcome(SurveyResultDto? Result, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static SubmissionOutcome Success(SurveyResultDto result) =>
        new(result, new Dictionary<string, string[]>());

    public static SubmissionOutcome Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors);
}
