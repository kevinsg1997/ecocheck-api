using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoCheck.Api.Data.Configurations;

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        builder.ToTable("survey_responses");

        // O Id (Guid v7) é gerado pela aplicação.
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Percentage).HasPrecision(5, 2);
        builder.Property(r => r.Classification).HasConversion<string>().HasMaxLength(32);

        builder.HasMany(r => r.Answers)
            .WithOne(a => a.SurveyResponse)
            .HasForeignKey(a => a.SurveyResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.CategoryScores)
            .WithOne(c => c.SurveyResponse)
            .HasForeignKey(c => c.SurveyResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Classification);
    }
}
