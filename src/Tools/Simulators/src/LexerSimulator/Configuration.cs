using System.Text.Json.Serialization;

namespace Luna.Compilers.Tools;

#pragma warning disable CS0649
internal class Configuration
{
    private readonly string[]? paths;
    public string[] Paths => paths ?? Array.Empty<string>();

    private readonly IDictionary<string, string[]>? extensions;
    public IDictionary<string, string[]> Extensions => this.extensions ?? new Dictionary<string, string[]>(0);
}
#pragma warning restore CS0649
