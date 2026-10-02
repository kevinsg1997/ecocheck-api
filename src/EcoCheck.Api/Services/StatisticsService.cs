using EcoCheck.Api.Data;
using EcoCheck.Api.Dtos;
using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EcoCheck.Api.Services;

public class StatisticsService(
    AppDbContext db,
    IMemoryCache cache,
    StatisticsCacheSignal cacheSignal,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public async Task<StatisticsDto> GetAsync(StatisticsFilter filter, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(filter.CacheKey, out StatisticsDto? cached) && cached is not null)
        {
            return cached;
        }

        var statistics = await ComputeAsync(filter, cancellationToken);

        using (var entry = cache.CreateEntry(filter.CacheKey))
        {
            entry.Value = statistics;
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            entry.AddExpirationToken(cacheSignal.CreateToken());
        }

        return statistics;
    }

    /// <summary>
    /// Chamado após uma nova participação, para que a pessoa já se veja incluída nas estatísticas
    /// (de todos os filtros).
    /// </summary>
    public void Invalidate() => cacheSignal.Reset();

    private async Task<StatisticsDto> ComputeAsync(StatisticsFilter filter, CancellationToken cancellationToken)
    {
        var responses = ApplyFilter(db.SurveyResponses, filter);
        var responseIds = responses.Select(r => r.Id);

        var totalParticipants = await responses.CountAsync(cancellationToken);

        var averagePercentage = await responses
            .AverageAsync(r => (decimal?)r.Percentage, cancellationToken);

        var categories = await db.SurveyCategoryScores
            .Where(c => responseIds.Contains(c.SurveyResponseId))
            .GroupBy(c => c.Category)
            .Select(g => new CategoryAggregate(g.Key, g.Average(c => c.Percentage)))
            .ToListAsync(cancellationToken);

        var classifications = await responses
            .GroupBy(r => r.Classification)
            .Select(g => new ClassificationAggregate(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var optionCounts = await db.SurveyAnswers
            .Where(a => responseIds.Contains(a.SurveyResponseId))
            .GroupBy(a => a.QuestionOptionId)
            .Select(g => new OptionAggregate(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var countries = await responses
            .Where(r => r.CountryCode != null)
            .GroupBy(r => r.CountryCode!)
            .Select(g => new RegionAggregate(g.Key, g.Count(), g.Average(r => r.Percentage)))
            .ToListAsync(cancellationToken);

        var brazilStates = await responses
            .Where(r => r.CountryCode == RegionCatalog.Brazil && r.StateCode != null)
            .GroupBy(r => r.StateCode!)
            .Select(g => new RegionAggregate(g.Key, g.Count(), g.Average(r => r.Percentage)))
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
            timeProvider.GetUtcNow(),
            countries,
            brazilStates,
            filter));
    }

    private static IQueryable<SurveyResponse> ApplyFilter(IQueryable<SurveyResponse> responses, StatisticsFilter filter)
    {
        if (filter.CountryCode is not null)
        {
            responses = responses.Where(r => r.CountryCode == filter.CountryCode);
        }

        if (filter.StateCode is not null)
        {
            responses = responses.Where(r => r.StateCode == filter.StateCode);
        }

        return responses;
    }
}
