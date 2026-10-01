using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Services;

/// <summary>
/// Faixas: [0, 20], (20, 40], (40, 60], (60, 80], (80, 100].
/// </summary>
public static class ClassificationRules
{
    public static Classification FromPercentage(decimal percentage) => percentage switch
    {
        < 0 or > 100 => throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "O percentual deve estar entre 0 e 100."),
        <= 20 => Classification.Starting,
        <= 40 => Classification.FirstSteps,
        <= 60 => Classification.OnTrack,
        <= 80 => Classification.GoodHabits,
        _ => Classification.Inspiring
    };
}
