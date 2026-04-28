using EdTech.SheetIntegrator.Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.CreatedAt).IsRequired();

        // Domain events are an in-memory concern; never persist them.
        builder.Ignore(a => a.DomainEvents);

        // MaxScore is a computed projection over Questions; do not persist it.
        builder.Ignore(a => a.MaxScore);

        // Questions: collection of value objects -> JSON column on the Assessments table.
        builder.OwnsMany(a => a.Questions, q =>
        {
            q.ToJson("Questions");
            q.Property(x => x.QuestionId).HasMaxLength(100);
            q.Property(x => x.Prompt).HasMaxLength(2000);
            q.Property(x => x.CorrectAnswer).HasMaxLength(2000);
        });
    }
}
