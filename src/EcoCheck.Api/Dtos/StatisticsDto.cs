using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Dtos;

/// <summary>
/// Estatísticas agregadas das participações (todas ou de uma região). Não contém nenhum dado individual.
/// </summary>
/// <param name="SummaryAvailable">
/// Falso quando um filtro de região tem menos participantes que o mínimo: nesse caso nenhum
/// agregado é retornado (inclusive <c>TotalParticipants</c>, que vem como 0).
/// </param>
public record StatisticsDto(
    StatisticsFilterDto Filter,
    bool SummaryAvailable,
    int TotalParticipants,
    decimal? AveragePercentage,
    IReadOnlyList<CategoryAverageDto> Categories,
    IReadOnlyList<ClassificationCountDto> Classifications,
    bool DetailsAvailable,
    int MinimumParticipantsForDetails,
    IReadOnlyList<QuestionStatisticsDto> Questions,
    IReadOnlyList<HabitHighlightDto> TopHabits,
    IReadOnlyList<HabitHighlightDto> ImprovementOpportunities,
    RegionStatisticsDto Regions,
    DateTimeOffset GeneratedAt);

/// <summary>
/// Participação por região. Só aparecem grupos com o mínimo de participantes,
/// para que regiões com poucas respostas não revelem resultados individuais.
/// </summary>
public record RegionStatisticsDto(
    int ParticipantsWithRegion,
    IReadOnlyList<RegionGroupDto> Countries,
    IReadOnlyList<RegionGroupDto> BrazilStates);

public record RegionGroupDto(string Code, int Participants, decimal AveragePercentage);

public record StatisticsFilterDto(string? CountryCode, string? StateCode);

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
