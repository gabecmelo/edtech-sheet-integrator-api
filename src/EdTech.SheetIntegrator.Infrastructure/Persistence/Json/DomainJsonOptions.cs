using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Json;

/// <summary>
/// JSON serializer options used for persisting domain value objects to JSON columns.
/// Settings are intentionally explicit (not relying on defaults) so a future framework
/// upgrade can't silently change the storage format.
/// </summary>
internal static class DomainJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };
}
