namespace EcoCheck.Api.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Pontuação da alternativa (0 a 4). Nulo significa "Não se aplica":
    /// a pergunta é desconsiderada no cálculo do máximo possível.
    /// </summary>
    public int? Points { get; set; }

    public int DisplayOrder { get; set; }
}
