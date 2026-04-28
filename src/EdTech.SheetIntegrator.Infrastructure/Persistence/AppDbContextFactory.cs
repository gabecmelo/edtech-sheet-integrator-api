using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core CLI tooling (<c>dotnet ef migrations add</c>, <c>dotnet ef database update</c>)
/// to construct an <see cref="AppDbContext"/> at design time without bringing up the full host.
/// The connection string need not point at a real database for migration generation.
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=EdTechSheetIntegrator_Design;Trusted_Connection=True;",
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

        return new AppDbContext(optionsBuilder.Options);
    }
}
