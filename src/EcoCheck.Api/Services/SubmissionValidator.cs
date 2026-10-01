using EcoCheck.Api.Dtos;
using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Services;

/// <summary>
/// Valida as respostas enviadas contra as perguntas ativas do questionário.
/// Retorna um dicionário de erros no formato de ValidationProblemDetails (vazio = válido).
/// </summary>
public static class SubmissionValidator
{
    public static Dictionary<string, string[]> Validate(
        SubmitResponseRequest request,
        IReadOnlyList<Question> activeQuestions,
        int currentVersion)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.QuestionnaireVersion != currentVersion)
        {
            errors["questionnaireVersion"] =
                ["O questionário foi atualizado. Recarregue a página para responder à versão mais recente."];
            return errors;
        }

        var messages = new List<string>();
        var questionsById = activeQuestions.ToDictionary(q => q.Id);

        if (request.Answers.GroupBy(a => a.QuestionId).Any(g => g.Count() > 1))
        {
            messages.Add("Cada pergunta deve ser respondida apenas uma vez.");
        }

        if (request.Answers.Any(a => !questionsById.ContainsKey(a.QuestionId)))
        {
            messages.Add("Há respostas para perguntas inexistentes ou inativas.");
        }

        var answeredIds = request.Answers.Select(a => a.QuestionId).ToHashSet();
        if (activeQuestions.Any(q => !answeredIds.Contains(q.Id)))
        {
            messages.Add("Responda todas as perguntas antes de enviar.");
        }

        var hasInvalidOption = request.Answers.Any(a =>
            questionsById.TryGetValue(a.QuestionId, out var question) &&
            question.Options.All(o => o.Id != a.OptionId));
        if (hasInvalidOption)
        {
            messages.Add("Há alternativas que não pertencem à pergunta indicada.");
        }

        if (messages.Count > 0)
        {
            errors["answers"] = [.. messages];
        }

        return errors;
    }
}
