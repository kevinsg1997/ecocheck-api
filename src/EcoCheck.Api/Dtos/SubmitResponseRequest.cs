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
}

public class AnswerRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Pergunta inválida.")]
    public int QuestionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Alternativa inválida.")]
    public int OptionId { get; set; }
}
