using EcoCheck.Api.Data;
using EcoCheck.Api.Dtos;
using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoCheck.Api.Services;

public class SurveyService(AppDbContext db, ScoringService scoring, TimeProvider timeProvider)
{
    public async Task<QuestionnaireDto> GetQuestionnaireAsync(CancellationToken cancellationToken)
    {
        var questions = await LoadActiveQuestionsAsync(cancellationToken);

        var questionDtos = questions
            .Select(q => new QuestionDto(
                q.Id,
                q.Category,
                q.Text,
                q.DisplayOrder,
                q.Options
                    .Select(o => new OptionDto(o.Id, o.Text, IsNotApplicable: o.Points is null))
                    .ToList()))
            .ToList();

        return new QuestionnaireDto(QuestionnaireInfo.CurrentVersion, questionDtos);
    }

    public async Task<SubmissionOutcome> SubmitAsync(SubmitResponseRequest request, CancellationToken cancellationToken)
    {
        var questions = await LoadActiveQuestionsAsync(cancellationToken);

        var errors = SubmissionValidator.Validate(request, questions, QuestionnaireInfo.CurrentVersion);
        if (errors.Count > 0)
        {
            return SubmissionOutcome.Invalid(errors);
        }

        var questionsById = questions.ToDictionary(q => q.Id);
        var chosen = request.Answers
            .Select(a =>
            {
                var question = questionsById[a.QuestionId];
                var option = question.Options.Single(o => o.Id == a.OptionId);
                return (Question: question, Option: option);
            })
            .ToList();

        var score = scoring.Calculate(chosen
            .Select(c => new ScoredAnswer(
                c.Question.Id,
                c.Question.Category,
                c.Option.Points,
                c.Question.Options.Max(o => o.Points ?? 0)))
            .ToList());

        var now = timeProvider.GetUtcNow();
        var response = new SurveyResponse
        {
            Id = Guid.CreateVersion7(now),
            CreatedAt = now.UtcDateTime,
            QuestionnaireVersion = QuestionnaireInfo.CurrentVersion,
            TotalScore = score.TotalScore,
            MaxScore = score.MaxScore,
            Percentage = score.Percentage,
            Classification = score.Classification,
            CountryCode = RegionCatalog.Normalize(request.CountryCode),
            StateCode = RegionCatalog.Normalize(request.StateCode),
            Answers = chosen
                .Select(c => new SurveyAnswer
                {
                    QuestionId = c.Question.Id,
                    QuestionOptionId = c.Option.Id,
                    Points = c.Option.Points
                })
                .ToList(),
            CategoryScores = score.Categories
                .Select(c => new SurveyCategoryScore
                {
                    Category = c.Category,
                    Score = c.Score,
                    MaxScore = c.MaxScore,
                    Percentage = c.Percentage
                })
                .ToList()
        };

        db.SurveyResponses.Add(response);
        await db.SaveChangesAsync(cancellationToken);

        return SubmissionOutcome.Success(new SurveyResultDto(
            score.TotalScore,
            score.MaxScore,
            score.Percentage,
            score.Classification,
            score.Categories
                .Select(c => new CategoryResultDto(c.Category, c.Score, c.MaxScore, c.Percentage))
                .ToList(),
            score.StrengthQuestionIds,
            score.ImprovementQuestionIds));
    }

    private Task<List<Question>> LoadActiveQuestionsAsync(CancellationToken cancellationToken) =>
        db.Questions
            .AsNoTracking()
            .Where(q => q.IsActive)
            .Include(q => q.Options.OrderBy(o => o.DisplayOrder))
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync(cancellationToken);
}
