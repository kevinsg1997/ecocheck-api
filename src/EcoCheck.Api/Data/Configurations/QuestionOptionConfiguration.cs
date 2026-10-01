using EcoCheck.Api.Data.Seed;
using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoCheck.Api.Data.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("question_options", t =>
            t.HasCheckConstraint("ck_question_options_points", "points IS NULL OR (points >= 0 AND points <= 4)"));

        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Text).HasMaxLength(150).IsRequired();

        builder.HasData(QuestionnaireSeed.Options);
    }
}
