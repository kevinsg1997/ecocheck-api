using EcoCheck.Api.Data;
using EcoCheck.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EcoCheck.Api.Services;

public class StatisticsService(AppDbContext db, IMemoryCache cache, TimeProvider timeProvider)
{
    private const string CacheKey = "statistics";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public async Task<StatisticsDto> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out StatisticsDto? cached) && cached is not null)
        {
            return cached;
        }

        var statistics = await ComputeAsync(cancellationToken);
        cache.Set(CacheKey, statistics, CacheDuration);
        return statistics;
    }

    /// <summary>
    /// Chamado após uma nova participação, para que a pessoa já se veja incluída nas estatísticas.
    /// </summary>
    public void Invalidate() => cache.Remove(CacheKey);

    private async Task<StatisticsDto> ComputeAsync(CancellationToken cancellationToken)
    {
        var totalParticipants = await db.SurveyResponses.CountAsync(cancellationToken);

        var averagePercentage = await db.SurveyResponses
            .AverageAsync(r => (decimal?)r.Percentage, cancellationToken);

        var categories = await db.SurveyCategoryScores
            .GroupBy(c => c.Category)
            .Select(g => new CategoryAggregate(g.Key, g.Average(c => c.Percentage)))
            .ToListAsync(cancellationToken);

        var classifications = await db.SurveyResponses
            .GroupBy(r => r.Classification)
            .Select(g => new ClassificationAggregate(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var optionCounts = await db.SurveyAnswers
            .GroupBy(a => a.QuestionOptionId)
            .Select(g => new OptionAggregate(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var activeQuestions = await db.Questions
            .AsNoTracking()
            .Where(q => q.IsActive)
            .Include(q => q.Options)
            .ToListAsync(cancellationToken);

        return StatisticsBuilder.Build(new StatisticsInput(
            totalParticipants,
            averagePercentage,
            categories,
            classifications,
            optionCounts,
            activeQuestions,
            timeProvider.GetUtcNow()));
    }
}
