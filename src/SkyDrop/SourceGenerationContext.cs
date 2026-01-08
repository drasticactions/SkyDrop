using System.Text.Json.Serialization;
using FishyFlip.Models;
using SkyDrop.Models;

namespace SkyDrop;

[JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(Session))]
[JsonSerializable(typeof(JmdictDictionary))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}