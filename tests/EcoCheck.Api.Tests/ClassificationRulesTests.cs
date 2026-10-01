using EcoCheck.Api.Entities;
using EcoCheck.Api.Services;

namespace EcoCheck.Api.Tests;

public class ClassificationRulesTests
{
    [Theory]
    [InlineData(0, Classification.Starting)]
    [InlineData(20, Classification.Starting)]
    [InlineData(20.01, Classification.FirstSteps)]
    [InlineData(40, Classification.FirstSteps)]
    [InlineData(40.5, Classification.OnTrack)]
    [InlineData(60, Classification.OnTrack)]
    [InlineData(60.01, Classification.GoodHabits)]
    [InlineData(80, Classification.GoodHabits)]
    [InlineData(80.01, Classification.Inspiring)]
    [InlineData(100, Classification.Inspiring)]
    public void FromPercentage_ReturnsExpectedRange(double percentage, Classification expected)
    {
        Assert.Equal(expected, ClassificationRules.FromPercentage((decimal)percentage));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void FromPercentage_OutOfRange_Throws(double percentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassificationRules.FromPercentage((decimal)percentage));
    }
}
