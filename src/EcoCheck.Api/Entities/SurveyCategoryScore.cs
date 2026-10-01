namespace EcoCheck.Api.Entities;

public class SurveyCategoryScore
{
    public Guid SurveyResponseId { get; set; }
    public SurveyResponse SurveyResponse { get; set; } = null!;
    public Category Category { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
}
