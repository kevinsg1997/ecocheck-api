using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Dtos;

public record QuestionnaireDto(int Version, IReadOnlyList<QuestionDto> Questions);

public record QuestionDto(int Id, Category Category, string Text, int Order, IReadOnlyList<OptionDto> Options);

/// <summary>
/// A pontuação não é exposta, para que a pessoa responda com sinceridade.
/// </summary>
public record OptionDto(int Id, string Text, bool IsNotApplicable);
