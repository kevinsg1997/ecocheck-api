using EcoCheck.Api.Data.Seed;
using EcoCheck.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoCheck.Api.Data.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        // Ids definidos no seed, sem auto incremento.
        builder.Property(q => q.Id).ValueGeneratedNever();
        builder.Property(q => q.Category).HasConversion<string>().HasMaxLength(32);
        builder.Property(q => q.Text).HasMaxLength(300).IsRequired();

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => new { q.Category, q.DisplayOrder });

        builder.HasData(QuestionnaireSeed.Questions);
    }
}
