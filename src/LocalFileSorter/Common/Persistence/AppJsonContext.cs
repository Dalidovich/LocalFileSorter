using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalFileSorter.Common.Persistence;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public sealed partial class AppJsonContext : JsonSerializerContext
{
}
