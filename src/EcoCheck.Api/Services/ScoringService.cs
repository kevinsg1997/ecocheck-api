using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Services;

/// <summary>Resposta já associada à pergunta, pronta para o cálculo.</summary>
/// <param name="Points">Pontos da alternativa escolhida; null = "Não se aplica".</param>
/// <param name="MaxPoints">Maior pontuação possível na pergunta.</param>
public record ScoredAnswer(int QuestionId, Category Category, int? Points, int MaxPoints);

public record CategoryScore(Category Category, int Score, int MaxScore, decimal Percentage);

public record ScoreResult(
    int TotalScore,
    int MaxScore,
    decimal Percentage,
    Classification Classification,
    IReadOnlyList<CategoryScore> Categories,
    IReadOnlyList<int> StrengthQuestionIds,
    IReadOnlyList<int> ImprovementQuestionIds);

/// <summary>
/// Cálculo puro da pontuação, sem acesso a banco. Perguntas respondidas com
/// "Não se aplica" são excluídas tanto da pontuação quanto do máximo possível.
/// </summary>
public class ScoringService
{
    /// <summary>Até esta fração do máximo, a pergunta é tratada como oportunidade de melhoria.</summary>
    private const decimal ImprovementThreshold = 0.5m;

    public ScoreResult Calculate(IReadOnlyCollection<ScoredAnswer> answers)
    {
        var applicable = answers.Where(a => a.Points.HasValue).ToList();

        var maxScore = applicable.Sum(a => a.MaxPoints);
        if (maxScore <= 0)
        {
            throw new InvalidOperationException("Não há respostas pontuáveis para calcular o resultado.");
        }

        var totalScore = applicable.Sum(a => a.Points!.Value);
        var percentage = ToPercentage(totalScore, maxScore);

        // Categorias sem nenhuma pergunta aplicável ficam de fora do resultado.
        var categories = applicable
            .GroupBy(a => a.Category)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var score = g.Sum(a => a.Points!.Value);
                var max = g.Sum(a => a.MaxPoints);
                return new CategoryScore(g.Key, score, max, ToPercentage(score, max));
            })
            .ToList();

        var strengths = applicable
            .Where(a => a.Points == a.MaxPoints)
            .Select(a => a.QuestionId)
            .ToList();

        var improvements = applicable
            .Where(a => a.Points!.Value <= a.MaxPoints * ImprovementThreshold)
            .OrderBy(a => (decimal)a.Points!.Value / a.MaxPoints)
            .ThenBy(a => a.QuestionId)
            .Select(a => a.QuestionId)
            .ToList();

        return new ScoreResult(
            totalScore,
            maxScore,
            percentage,
            ClassificationRules.FromPercentage(percentage),
            categories,
            strengths,
            improvements);
    }

    private static decimal ToPercentage(int score, int max) =>
        Math.Round(score * 100m / max, 2, MidpointRounding.AwayFromZero);
}
