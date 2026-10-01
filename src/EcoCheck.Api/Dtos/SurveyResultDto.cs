using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Dtos;

public record SurveyResultDto(
    int TotalScore,
    int MaxScore,
    decimal Percentage,
    Classification Classification,
    IReadOnlyList<CategoryResultDto> Categories,
    IReadOnlyList<int> StrengthQuestionIds,
    IReadOnlyList<int> ImprovementQuestionIds);

public record CategoryResultDto(Category Category, int Score, int MaxScore, decimal Percentage);
