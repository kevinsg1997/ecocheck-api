using System.ComponentModel.DataAnnotations;

namespace EcoCheck.Api.Dtos;

public class SubmitResponseRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Versão do questionário inválida.")]
    public int QuestionnaireVersion { get; set; }

    [Required(ErrorMessage = "As respostas são obrigatórias.")]
    [MinLength(1, ErrorMessage = "Envie ao menos uma resposta.")]
    [MaxLength(50, ErrorMessage = "Quantidade de respostas acima do permitido.")]
    public List<AnswerRequest> Answers { get; set; } = [];

    /// <summary>Opcional. Código do país (ISO 3166-1 alfa-2), ex.: "BR".</summary>
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "País inválido.")]
    public string? CountryCode { get; set; }

    /// <summary>Opcional. Sigla da UF, aceita somente quando o país é o Brasil, ex.: "SP".</summary>
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Estado inválido.")]
    public string? StateCode { get; set; }
}

public class AnswerRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Pergunta inválida.")]
    public int QuestionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Alternativa inválida.")]
    public int OptionId { get; set; }
}
