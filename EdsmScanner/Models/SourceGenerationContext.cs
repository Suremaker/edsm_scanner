using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdsmScanner.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SystemRef))]
[JsonSerializable(typeof(SystemInfo))]
[JsonSerializable(typeof(SystemDetails))]
[JsonSerializable(typeof(SystemBody))]
[JsonSerializable(typeof(RingInfo))]
[JsonSerializable(typeof(CoordF))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}