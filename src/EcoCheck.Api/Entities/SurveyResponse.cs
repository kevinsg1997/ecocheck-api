namespace EcoCheck.Api.Entities;

/// <summary>
/// Uma participação anônima no questionário. Não possui nenhum dado pessoal.
/// </summary>
public class SurveyResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionnaireVersion { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public Classification Classification { get; set; }

    public List<SurveyAnswer> Answers { get; set; } = [];
    public List<SurveyCategoryScore> CategoryScores { get; set; } = [];
}
