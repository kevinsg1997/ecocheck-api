using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoCheck.Api.Data.Configurations;

public class SurveyCategoryScoreConfiguration : IEntityTypeConfiguration<SurveyCategoryScore>
{
    public void Configure(EntityTypeBuilder<SurveyCategoryScore> builder)
    {
        builder.ToTable("survey_category_scores");

        builder.HasKey(c => new { c.SurveyResponseId, c.Category });
        builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.Percentage).HasPrecision(5, 2);

        builder.HasIndex(c => c.Category);
    }
}
