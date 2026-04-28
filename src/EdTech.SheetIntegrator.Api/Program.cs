using System.Text;
using EdTech.SheetIntegrator.Api.Auth;
using EdTech.SheetIntegrator.Api.Endpoints;
using EdTech.SheetIntegrator.Api.Middleware;
using EdTech.SheetIntegrator.Application.DependencyInjection;
using EdTech.SheetIntegrator.Infrastructure.DependencyInjection;
using EdTech.SheetIntegrator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ----- Logging (Serilog) -----
builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));

// ----- Configuration: strongly typed JWT options -----
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ----- Auth: JWT Bearer + Instructor policy -----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(JwtTokenService.InstructorPolicy, policy =>
        policy.RequireAuthenticatedUser().RequireRole(JwtTokenService.InstructorRole));

builder.Services.AddSingleton<JwtTokenService>();

// ----- Application + Infrastructure -----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ----- Health checks -----
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

// ----- Upload size cap from config -----
var maxUploadBytes = builder.Configuration.GetValue<long?>("Upload:MaxFileSizeBytes") ?? 10L * 1024L * 1024L;
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = maxUploadBytes;
});
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = maxUploadBytes);

// ----- Problem details + OpenAPI -----
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi("v1");
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ----- Pipeline -----
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", opts => opts
        .WithTitle("EdTech Sheet-Integrator API")
        .WithTheme(ScalarTheme.Purple));
}

app.UseAuthentication();
app.UseAuthorization();

// ----- Endpoints -----
var v1 = app.MapGroup("/api/v1").WithOpenApi();

v1.MapGroup("/assessments")
    .WithTags("Assessments")
    .MapAssessmentsEndpoints()
    .MapAssessmentSubmissionsEndpoints();

v1.MapGroup("/submissions")
    .WithTags("Submissions")
    .MapSubmissionsEndpoints();

app.MapHealthEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapDevAuthEndpoints();
}

app.Run();

// Required so WebApplicationFactory<Program> can resolve the entry-point type in Part 8 tests.
public partial class Program;
