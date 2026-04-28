using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Infrastructure.Persistence;
using EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;
using EdTech.SheetIntegrator.Infrastructure.Sheets;
using EdTech.SheetIntegrator.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdTech.SheetIntegrator.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core (SQL Server), repositories, the unit-of-work, the system clock,
    /// and both sheet parsers behind <see cref="ISheetParser"/> + <see cref="ISheetParserFactory"/>.
    /// Reads the connection string from configuration key <c>ConnectionStrings:Default</c>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<ISheetParser, ClosedXmlSheetParser>();
        services.AddSingleton<ISheetParser, CsvHelperSheetParser>();
        services.AddSingleton<ISheetParserFactory, SheetParserFactory>();

        return services;
    }
}
