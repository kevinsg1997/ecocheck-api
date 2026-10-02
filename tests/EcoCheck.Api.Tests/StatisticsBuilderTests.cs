using EcoCheck.Api.Entities;
using EcoCheck.Api.Services;

namespace EcoCheck.Api.Tests;

public class StatisticsBuilderTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    // Pergunta 1: alternativas 11 (4 pts), 12 (0 pts), 13 (não se aplica).
    // Pergunta 2: alternativas 21 (4 pts), 22 (2 pts).
    // Pergunta 3..5: alternativas X1 (4 pts), X2 (0 pts).
    private static readonly IReadOnlyList<Question> Questions =
    [
        NewQuestion(1, Category.Water, (11, 4), (12, 0), (13, null)),
        NewQuestion(2, Category.Energy, (21, 4), (22, 2)),
        NewQuestion(3, Category.Waste, (31, 4), (32, 0)),
        NewQuestion(4, Category.Waste, (41, 4), (42, 0)),
        NewQuestion(5, Category.ConsumptionAndMobility, (51, 4), (52, 0))
    ];

    private static Question NewQuestion(int id, Category category, params (int Id, int? Points)[] options) => new()
    {
        Id = id,
        Category = category,
        Text = $"Pergunta {id}",
        DisplayOrder = id,
        Options = options
            .Select((o, i) => new QuestionOption { Id = o.Id, QuestionId = id, Text = $"Opção {o.Id}", Points = o.Points, DisplayOrder = i + 1 })
            .ToList()
    };

    private static StatisticsInput Input(int participants, params OptionAggregate[] counts) => new(
        participants,
        participants > 0 ? 55.555m : null,
        [new CategoryAggregate(Category.Water, 70.126m), new CategoryAggregate(Category.Energy, 40m)],
        [new ClassificationAggregate(Classification.GoodHabits, participants)],
        counts,
        Questions,
        Now);

    [Fact]
    public void Build_NoParticipants_ReturnsEmptyAggregates()
    {
        var result = StatisticsBuilder.Build(new StatisticsInput(0, null, [], [], [], Questions, Now));

        Assert.Equal(0, result.TotalParticipants);
        Assert.Null(result.AveragePercentage);
        Assert.False(result.DetailsAvailable);
        Assert.Empty(result.Questions);
        Assert.All(result.Classifications, c => Assert.Equal(0, c.Count));
    }

    [Fact]
    public void Build_AlwaysReturnsAllClassifications()
    {
        var result = StatisticsBuilder.Build(Input(2));

        Assert.Equal(Enum.GetValues<Classification>().Length, result.Classifications.Count);
        var goodHabits = Assert.Single(result.Classifications, c => c.Classification == Classification.GoodHabits);
        Assert.Equal(2, goodHabits.Count);
        Assert.Equal(100m, goodHabits.Percentage);
    }

    [Fact]
    public void Build_RoundsAverages()
    {
        var result = StatisticsBuilder.Build(Input(2));

        Assert.Equal(55.56m, result.AveragePercentage);
        Assert.Equal(70.13m, result.Categories.Single(c => c.Category == Category.Water).AveragePercentage);
    }

    [Fact]
    public void Build_BelowMinimumParticipants_HidesQuestionDetails()
    {
        var result = StatisticsBuilder.Build(Input(StatisticsBuilder.MinimumParticipantsForDetails - 1, new OptionAggregate(11, 4)));

        Assert.False(result.DetailsAvailable);
        Assert.Empty(result.Questions);
        Assert.Empty(result.TopHabits);
        Assert.Empty(result.ImprovementOpportunities);
    }

    [Fact]
    public void Build_WithEnoughParticipants_CalculatesOptionDistributionAndAverage()
    {
        // Pergunta 1: 3x 4 pts, 1x 0 pts, 1x não se aplica.
        var result = StatisticsBuilder.Build(Input(5,
            new OptionAggregate(11, 3), new OptionAggregate(12, 1), new OptionAggregate(13, 1)));

        Assert.True(result.DetailsAvailable);

        var question = result.Questions.Single(q => q.QuestionId == 1);
        Assert.Equal(5, question.TotalAnswers);
        // "Não se aplica" fica fora da média: 12 pts de 16 possíveis.
        Assert.Equal(75m, question.AveragePercentage);
        Assert.Equal(60m, question.Options.Single(o => o.OptionId == 11).Percentage);
        Assert.True(question.Options.Single(o => o.OptionId == 13).IsNotApplicable);
    }

    [Fact]
    public void Build_QuestionWithoutAnswers_HasNullAverage()
    {
        var result = StatisticsBuilder.Build(Input(5, new OptionAggregate(11, 5)));

        var question = result.Questions.Single(q => q.QuestionId == 2);
        Assert.Equal(0, question.TotalAnswers);
        Assert.Null(question.AveragePercentage);
        Assert.All(question.Options, o => Assert.Equal(0m, o.Percentage));
    }

    [Fact]
    public void Build_Regions_OnlyShowsGroupsWithMinimumParticipants()
    {
        var input = Input(20) with
        {
            Countries = [new RegionAggregate("BR", 12, 61.234m), new RegionAggregate("PT", 4, 70m)],
            BrazilStates = [new RegionAggregate("SP", 5, 58m), new RegionAggregate("RJ", 7, 64m), new RegionAggregate("AC", 1, 90m)]
        };

        var regions = StatisticsBuilder.Build(input).Regions;

        Assert.Equal(16, regions.ParticipantsWithRegion);
        var brazil = Assert.Single(regions.Countries);
        Assert.Equal("BR", brazil.Code);
        Assert.Equal(61.23m, brazil.AveragePercentage);
        // Ordenado por número de participantes; AC (1) e PT (4) ficam ocultos.
        Assert.Equal(new[] { "RJ", "SP" }, regions.BrazilStates.Select(s => s.Code));
    }

    [Fact]
    public void Build_FilteredRegionBelowMinimum_SuppressesAllAggregates()
    {
        var input = Input(StatisticsBuilder.MinimumParticipantsForDetails - 1, new OptionAggregate(11, 4)) with
        {
            Filter = new StatisticsFilter("BR", "AC")
        };

        var result = StatisticsBuilder.Build(input);

        Assert.False(result.SummaryAvailable);
        Assert.Equal(0, result.TotalParticipants);
        Assert.Null(result.AveragePercentage);
        Assert.Empty(result.Categories);
        Assert.Empty(result.Classifications);
        Assert.Empty(result.Questions);
        Assert.Equal("BR", result.Filter.CountryCode);
        Assert.Equal("AC", result.Filter.StateCode);
    }

    [Fact]
    public void Build_FilteredRegionWithEnoughParticipants_ReturnsSummary()
    {
        var input = Input(StatisticsBuilder.MinimumParticipantsForDetails, new OptionAggregate(11, 5)) with
        {
            Filter = new StatisticsFilter("BR")
        };

        var result = StatisticsBuilder.Build(input);

        Assert.True(result.SummaryAvailable);
        Assert.True(result.DetailsAvailable);
        Assert.Equal(5, result.TotalParticipants);
        Assert.Equal("BR", result.Filter.CountryCode);
        Assert.Null(result.Filter.StateCode);
    }

    [Fact]
    public void Build_WithoutFilter_FewParticipants_StillReturnsSummary()
    {
        var result = StatisticsBuilder.Build(Input(2));

        Assert.True(result.SummaryAvailable);
        Assert.Equal(2, result.TotalParticipants);
        Assert.False(result.DetailsAvailable);
    }

    [Fact]
    public void Build_NoRegionData_ReturnsEmptyRegions()
    {
        var regions = StatisticsBuilder.Build(Input(5)).Regions;

        Assert.Equal(0, regions.ParticipantsWithRegion);
        Assert.Empty(regions.Countries);
        Assert.Empty(regions.BrazilStates);
    }

    [Fact]
    public void Build_HighlightsBestAndWorstHabitsWithoutOverlap()
    {
        var result = StatisticsBuilder.Build(Input(5,
            new OptionAggregate(11, 5),                            // P1: 100%
            new OptionAggregate(21, 5),                            // P2: 100%
            new OptionAggregate(31, 4), new OptionAggregate(32, 1), // P3: 80%
            new OptionAggregate(41, 1), new OptionAggregate(42, 4), // P4: 20%
            new OptionAggregate(52, 5)));                          // P5: 0%

        Assert.Equal(new[] { 1, 2, 3 }, result.TopHabits.Select(h => h.QuestionId));
        Assert.Equal(new[] { 5, 4 }, result.ImprovementOpportunities.Select(h => h.QuestionId));
    }
}
