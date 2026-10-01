namespace EcoCheck.Api.Entities;

public class SurveyAnswer
{
    public long Id { get; set; }
    public Guid SurveyResponseId { get; set; }
    public SurveyResponse SurveyResponse { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int QuestionOptionId { get; set; }
    public QuestionOption QuestionOption { get; set; } = null!;

    /// <summary>
    /// Cópia da pontuação no momento da resposta, para preservar o histórico
    /// caso a pontuação da alternativa seja alterada no futuro.
    /// </summary>
    public int? Points { get; set; }
}
