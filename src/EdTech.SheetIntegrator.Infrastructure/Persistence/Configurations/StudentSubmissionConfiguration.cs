using System.Text.Json;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;
using EdTech.SheetIntegrator.Infrastructure.Persistence.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Configurations;

internal sealed class StudentSubmissionConfiguration : IEntityTypeConfiguration<StudentSubmission>
{
    public void Configure(EntityTypeBuilder<StudentSubmission> builder)
    {
        builder.ToTable("StudentSubmissions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.AssessmentId).IsRequired();
        builder.HasIndex(s => s.AssessmentId);

        builder.Property(s => s.StudentIdentifier).HasMaxLength(256).IsRequired();
        builder.Property(s => s.SourceFileName).HasMaxLength(256).IsRequired();
        builder.Property(s => s.SubmittedAt).IsRequired();

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.IsGraded);

        // Answers: collection of value objects -> JSON column.
        builder.OwnsMany(s => s.Answers, a =>
        {
            a.ToJson("Answers");
            a.Property(x => x.QuestionId).HasMaxLength(100);
            a.Property(x => x.Response).HasMaxLength(2000);
        });

        // Result: nullable GradingResult VO containing a Score struct + Outcomes collection +
        // GradedAt. Storing the whole VO as a single JSON column avoids the friction of mapping
        // a struct-based value object across owned-type / complex-type boundaries, and makes the
        // ungraded-vs-graded distinction explicit (NULL == not graded).
        var resultConverter = new ResultJsonConverter();
        builder.Property(s => s.Result)
            .HasConversion(resultConverter)
            .HasColumnName("ResultJson")
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ResultValueComparer());
    }

    private sealed class ResultJsonConverter
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<GradingResult?, string?>
    {
        public ResultJsonConverter()
            : base(
                v => v == null ? null : JsonSerializer.Serialize(v, DomainJsonOptions.Default),
                v => string.IsNullOrEmpty(v)
                    ? null
                    : JsonSerializer.Deserialize<GradingResult>(v, DomainJsonOptions.Default))
        {
        }
    }

    private sealed class ResultValueComparer : ValueComparer<GradingResult?>
    {
        public ResultValueComparer()
            : base(
                (l, r) => object.Equals(l, r),
                v => v == null ? 0 : v.GetHashCode(),
                v => v)
        {
        }
    }
}
