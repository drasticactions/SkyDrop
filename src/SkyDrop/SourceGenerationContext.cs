using System.Text.Json.Serialization;
using FishyFlip.Models;

namespace SkyDrop;

[JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(Session))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}