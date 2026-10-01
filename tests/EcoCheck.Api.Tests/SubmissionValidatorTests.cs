using EcoCheck.Api.Data.Seed;
using EcoCheck.Api.Dtos;
using EcoCheck.Api.Entities;
using EcoCheck.Api.Services;

namespace EcoCheck.Api.Tests;

public class SubmissionValidatorTests
{
    private const int Version = QuestionnaireInfo.CurrentVersion;

    // Cópias das perguntas do seed com suas alternativas, como viriam do banco.
    private static readonly IReadOnlyList<Question> Questions = QuestionnaireSeed.Questions
        .Select(q => new Question
        {
            Id = q.Id,
            Category = q.Category,
            Text = q.Text,
            DisplayOrder = q.DisplayOrder,
            IsActive = q.IsActive,
            Options = QuestionnaireSeed.Options
                .Where(o => o.QuestionId == q.Id)
                .Select(o => new QuestionOption
                {
                    Id = o.Id,
                    QuestionId = o.QuestionId,
                    Text = o.Text,
                    Points = o.Points,
                    DisplayOrder = o.DisplayOrder
                })
                .ToList()
        })
        .ToList();

    private static SubmitResponseRequest ValidRequest() => new()
    {
        QuestionnaireVersion = Version,
        Answers = Questions
            .Select(q => new AnswerRequest { QuestionId = q.Id, OptionId = q.Options[0].Id })
            .ToList()
    };

    [Fact]
    public void Validate_CompleteRequest_HasNoErrors()
    {
        var errors = SubmissionValidator.Validate(ValidRequest(), Questions, Version);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_OutdatedVersion_ReturnsVersionError()
    {
        var request = ValidRequest();
        request.QuestionnaireVersion = Version + 1;

        var errors = SubmissionValidator.Validate(request, Questions, Version);

        Assert.True(errors.ContainsKey("questionnaireVersion"));
    }

    [Fact]
    public void Validate_MissingAnswer_ReturnsError()
    {
        var request = ValidRequest();
        request.Answers.RemoveAt(0);

        var errors = SubmissionValidator.Validate(request, Questions, Version);

        Assert.Contains("Responda todas as perguntas antes de enviar.", errors["answers"]);
    }

    [Fact]
    public void Validate_DuplicateAnswer_ReturnsError()
    {
        var request = ValidRequest();
        request.Answers.Add(request.Answers[0]);

        var errors = SubmissionValidator.Validate(request, Questions, Version);

        Assert.Contains("Cada pergunta deve ser respondida apenas uma vez.", errors["answers"]);
    }

    [Fact]
    public void Validate_UnknownQuestion_ReturnsError()
    {
        var request = ValidRequest();
        request.Answers.Add(new AnswerRequest { QuestionId = 999, OptionId = 9991 });

        var errors = SubmissionValidator.Validate(request, Questions, Version);

        Assert.Contains("Há respostas para perguntas inexistentes ou inativas.", errors["answers"]);
    }

    [Fact]
    public void Validate_OptionFromAnotherQuestion_ReturnsError()
    {
        var request = ValidRequest();
        // Alternativa da pergunta 2 enviada como resposta da pergunta 1.
        request.Answers[0].OptionId = Questions[1].Options[0].Id;

        var errors = SubmissionValidator.Validate(request, Questions, Version);

        Assert.Contains("Há alternativas que não pertencem à pergunta indicada.", errors["answers"]);
    }
}
