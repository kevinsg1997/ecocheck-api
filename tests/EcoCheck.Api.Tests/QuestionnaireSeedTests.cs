using EcoCheck.Api.Data.Seed;
using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Tests;

/// <summary>
/// Garante a integridade do questionário: evita erros ao editar perguntas no seed.
/// </summary>
public class QuestionnaireSeedTests
{
    [Fact]
    public void Seed_Has20QuestionsWithUniqueIds()
    {
        Assert.Equal(20, QuestionnaireSeed.Questions.Count);
        Assert.Equal(20, QuestionnaireSeed.Questions.Select(q => q.Id).Distinct().Count());
    }

    [Fact]
    public void Seed_HasFiveQuestionsPerCategory()
    {
        foreach (var category in Enum.GetValues<Category>())
        {
            Assert.Equal(5, QuestionnaireSeed.Questions.Count(q => q.Category == category));
        }
    }

    [Fact]
    public void Seed_OptionIdsAreUniqueAndReferenceExistingQuestions()
    {
        var questionIds = QuestionnaireSeed.Questions.Select(q => q.Id).ToHashSet();

        Assert.Equal(QuestionnaireSeed.Options.Count, QuestionnaireSeed.Options.Select(o => o.Id).Distinct().Count());
        Assert.All(QuestionnaireSeed.Options, o => Assert.Contains(o.QuestionId, questionIds));
    }

    [Fact]
    public void Seed_EveryQuestionRangesFrom0To4()
    {
        foreach (var question in QuestionnaireSeed.Questions)
        {
            var points = QuestionnaireSeed.Options
                .Where(o => o.QuestionId == question.Id && o.Points.HasValue)
                .Select(o => o.Points!.Value)
                .ToList();

            Assert.True(points.Count >= 2, $"Pergunta {question.Id} precisa de ao menos duas alternativas pontuáveis.");
            Assert.Equal(4, points.Max());
            Assert.All(points, p => Assert.InRange(p, 0, 4));
        }
    }

    [Fact]
    public void Seed_EveryCategoryHasAQuestionWithoutNotApplicable()
    {
        // Garante que toda categoria sempre terá pontuação máxima maior que zero.
        foreach (var category in Enum.GetValues<Category>())
        {
            var hasMandatory = QuestionnaireSeed.Questions
                .Where(q => q.Category == category)
                .Any(q => QuestionnaireSeed.Options.Where(o => o.QuestionId == q.Id).All(o => o.Points.HasValue));

            Assert.True(hasMandatory, $"A categoria {category} precisa de ao menos uma pergunta sem 'Não se aplica'.");
        }
    }
}
