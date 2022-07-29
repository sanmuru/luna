using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Luna.Compilers.Tools;

#pragma warning disable CS0649
internal partial class SimulatorConfiguration
{
    private readonly string[]? paths;
    public string[] Paths => paths ?? Array.Empty<string>();

    private readonly IDictionary<string, string[]>? extensions;
    public IDictionary<string, string[]> Extensions => this.extensions ?? new Dictionary<string, string[]>(0);
}
#pragma warning restore CS0649

partial class SimulatorConfiguration
{
    public static SimulatorConfiguration? Deserialize(Stream stream)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        var doc = JsonDocument.Parse(stream);
        return doc.Deserialize<SimulatorConfiguration>(new JsonSerializerOptions()
        {
            IncludeFields = true,
            IgnoreReadOnlyProperties = true
        });
    }
}
