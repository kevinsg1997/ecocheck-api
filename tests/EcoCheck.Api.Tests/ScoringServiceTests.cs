using EcoCheck.Api.Entities;
using EcoCheck.Api.Services;

namespace EcoCheck.Api.Tests;

public class ScoringServiceTests
{
    private readonly ScoringService _scoring = new();

    [Fact]
    public void Calculate_AllMaxAnswers_Returns100PercentAndInspiring()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 4, 4),
            new ScoredAnswer(2, Category.Energy, 4, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.Equal(8, result.TotalScore);
        Assert.Equal(8, result.MaxScore);
        Assert.Equal(100m, result.Percentage);
        Assert.Equal(Classification.Inspiring, result.Classification);
    }

    [Fact]
    public void Calculate_AllZeroAnswers_Returns0PercentAndStarting()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 0, 4),
            new ScoredAnswer(2, Category.Waste, 0, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.Equal(0, result.TotalScore);
        Assert.Equal(0m, result.Percentage);
        Assert.Equal(Classification.Starting, result.Classification);
    }

    [Fact]
    public void Calculate_NotApplicableAnswer_IsExcludedFromScoreAndMax()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Energy, 4, 4),
            new ScoredAnswer(2, Category.Energy, null, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.Equal(4, result.TotalScore);
        Assert.Equal(4, result.MaxScore);
        Assert.Equal(100m, result.Percentage);
    }

    [Fact]
    public void Calculate_GroupsScoresByCategory()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 4, 4),
            new ScoredAnswer(2, Category.Water, 2, 4),
            new ScoredAnswer(3, Category.Energy, 1, 4)
        };

        var result = _scoring.Calculate(answers);

        var water = Assert.Single(result.Categories, c => c.Category == Category.Water);
        Assert.Equal(6, water.Score);
        Assert.Equal(8, water.MaxScore);
        Assert.Equal(75m, water.Percentage);

        var energy = Assert.Single(result.Categories, c => c.Category == Category.Energy);
        Assert.Equal(25m, energy.Percentage);
    }

    [Fact]
    public void Calculate_CategoryWithOnlyNotApplicable_IsOmitted()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 3, 4),
            new ScoredAnswer(2, Category.Energy, null, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.DoesNotContain(result.Categories, c => c.Category == Category.Energy);
    }

    [Fact]
    public void Calculate_RoundsPercentageToTwoDecimals()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 1, 4),
            new ScoredAnswer(2, Category.Water, 1, 4),
            new ScoredAnswer(3, Category.Water, 0, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.Equal(16.67m, result.Percentage);
    }

    [Fact]
    public void Calculate_IdentifiesStrengthsAndImprovements()
    {
        var answers = new[]
        {
            new ScoredAnswer(1, Category.Water, 4, 4),
            new ScoredAnswer(2, Category.Water, 3, 4),
            new ScoredAnswer(3, Category.Energy, 2, 4),
            new ScoredAnswer(4, Category.Energy, 0, 4),
            new ScoredAnswer(5, Category.Waste, null, 4)
        };

        var result = _scoring.Calculate(answers);

        Assert.Equal(new[] { 1 }, result.StrengthQuestionIds);
        // Ordenadas da menor para a maior pontuação relativa.
        Assert.Equal(new[] { 4, 3 }, result.ImprovementQuestionIds);
    }

    [Fact]
    public void Calculate_OnlyNotApplicableAnswers_Throws()
    {
        var answers = new[] { new ScoredAnswer(1, Category.Water, null, 4) };

        Assert.Throws<InvalidOperationException>(() => _scoring.Calculate(answers));
    }
}
