using EcoCheck.Api.Dtos;
using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Services;

public record CategoryAggregate(Category Category, decimal AveragePercentage);

public record ClassificationAggregate(Classification Classification, int Count);

public record OptionAggregate(int OptionId, int Count);

public record StatisticsInput(
    int TotalParticipants,
    decimal? AveragePercentage,
    IReadOnlyList<CategoryAggregate> Categories,
    IReadOnlyList<ClassificationAggregate> Classifications,
    IReadOnlyList<OptionAggregate> OptionCounts,
    IReadOnlyList<Question> ActiveQuestions,
    DateTimeOffset GeneratedAt);

/// <summary>
/// Monta o DTO de estatísticas a partir dos agregados do banco (sem acesso a banco).
/// Detalhes por pergunta só são exibidos a partir de um número mínimo de participantes,
/// para evitar que poucas respostas revelem escolhas individuais.
/// </summary>
public static class StatisticsBuilder
{
    public const int MinimumParticipantsForDetails = 5;
    private const int HighlightCount = 3;

    public static StatisticsDto Build(StatisticsInput input)
    {
        var categories = input.Categories
            .OrderBy(c => c.Category)
            .Select(c => new CategoryAverageDto(c.Category, Round(c.AveragePercentage)))
            .ToList();

        var countsByClassification = input.Classifications.ToDictionary(c => c.Classification, c => c.Count);
        var classifications = Enum.GetValues<Classification>()
            .Select(c =>
            {
                var count = countsByClassification.GetValueOrDefault(c);
                return new ClassificationCountDto(c, count, Percent(count, input.TotalParticipants));
            })
            .ToList();

        var detailsAvailable = input.TotalParticipants >= MinimumParticipantsForDetails;
        var questions = detailsAvailable ? BuildQuestions(input) : [];

        var ranked = questions
            .Where(q => q.AveragePercentage.HasValue)
            .Select(q => new HabitHighlightDto(q.QuestionId, q.Category, q.Text, q.AveragePercentage!.Value))
            .ToList();

        var topHabits = ranked
            .OrderByDescending(h => h.AveragePercentage)
            .ThenBy(h => h.QuestionId)
            .Take(HighlightCount)
            .ToList();

        var topIds = topHabits.Select(h => h.QuestionId).ToHashSet();
        var improvements = ranked
            .Where(h => !topIds.Contains(h.QuestionId))
            .OrderBy(h => h.AveragePercentage)
            .ThenBy(h => h.QuestionId)
            .Take(HighlightCount)
            .ToList();

        return new StatisticsDto(
            input.TotalParticipants,
            input.AveragePercentage.HasValue ? Round(input.AveragePercentage.Value) : null,
            categories,
            classifications,
            detailsAvailable,
            MinimumParticipantsForDetails,
            questions,
            topHabits,
            improvements,
            input.GeneratedAt);
    }

    private static List<QuestionStatisticsDto> BuildQuestions(StatisticsInput input)
    {
        var countsByOption = input.OptionCounts.ToDictionary(o => o.OptionId, o => o.Count);

        return input.ActiveQuestions
            .OrderBy(q => q.DisplayOrder)
            .Select(q =>
            {
                var options = q.Options.OrderBy(o => o.DisplayOrder).ToList();
                var total = options.Sum(o => countsByOption.GetValueOrDefault(o.Id));
                var maxPoints = options.Max(o => o.Points ?? 0);

                var applicableAnswers = options
                    .Where(o => o.Points.HasValue)
                    .Sum(o => countsByOption.GetValueOrDefault(o.Id));
                var earnedPoints = options
                    .Where(o => o.Points.HasValue)
                    .Sum(o => countsByOption.GetValueOrDefault(o.Id) * o.Points!.Value);

                decimal? average = applicableAnswers > 0 && maxPoints > 0
                    ? Round(earnedPoints * 100m / (applicableAnswers * maxPoints))
                    : null;

                var optionDtos = options
                    .Select(o =>
                    {
                        var count = countsByOption.GetValueOrDefault(o.Id);
                        return new OptionStatisticsDto(o.Id, o.Text, o.Points is null, count, Percent(count, total));
                    })
                    .ToList();

                return new QuestionStatisticsDto(q.Id, q.Category, q.Text, total, average, optionDtos);
            })
            .ToList();
    }

    private static decimal Percent(int count, int total) =>
        total == 0 ? 0 : Round(count * 100m / total);

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
