using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Dtos;

/// <summary>
/// Estatísticas agregadas de todas as participações. Não contém nenhum dado individual.
/// </summary>
public record StatisticsDto(
    int TotalParticipants,
    decimal? AveragePercentage,
    IReadOnlyList<CategoryAverageDto> Categories,
    IReadOnlyList<ClassificationCountDto> Classifications,
    bool DetailsAvailable,
    int MinimumParticipantsForDetails,
    IReadOnlyList<QuestionStatisticsDto> Questions,
    IReadOnlyList<HabitHighlightDto> TopHabits,
    IReadOnlyList<HabitHighlightDto> ImprovementOpportunities,
    DateTimeOffset GeneratedAt);

public record CategoryAverageDto(Category Category, decimal AveragePercentage);

public record ClassificationCountDto(Classification Classification, int Count, decimal Percentage);

public record QuestionStatisticsDto(
    int QuestionId,
    Category Category,
    string Text,
    int TotalAnswers,
    decimal? AveragePercentage,
    IReadOnlyList<OptionStatisticsDto> Options);

public record OptionStatisticsDto(int OptionId, string Text, bool IsNotApplicable, int Count, decimal Percentage);

public record HabitHighlightDto(int QuestionId, Category Category, string Text, decimal AveragePercentage);
