using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoCheck.Api.Data.Configurations;

public class SurveyAnswerConfiguration : IEntityTypeConfiguration<SurveyAnswer>
{
    public void Configure(EntityTypeBuilder<SurveyAnswer> builder)
    {
        builder.ToTable("survey_answers");

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.QuestionOption)
            .WithMany()
            .HasForeignKey(a => a.QuestionOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Uma resposta por pergunta em cada participação.
        builder.HasIndex(a => new { a.SurveyResponseId, a.QuestionId }).IsUnique();

        // Usado na distribuição de respostas por pergunta.
        builder.HasIndex(a => new { a.QuestionId, a.QuestionOptionId });
    }
}
