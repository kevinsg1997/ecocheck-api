namespace EcoCheck.Api.Entities;

public class Question
{
    public int Id { get; set; }
    public Category Category { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Perguntas nunca são apagadas; são desativadas para não quebrar estatísticas antigas.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public List<QuestionOption> Options { get; set; } = [];
}
