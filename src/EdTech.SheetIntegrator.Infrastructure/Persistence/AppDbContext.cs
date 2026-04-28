using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Submissions;
using Microsoft.EntityFrameworkCore;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Assessment> Assessments => Set<Assessment>();

    public DbSet<StudentSubmission> Submissions => Set<StudentSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
