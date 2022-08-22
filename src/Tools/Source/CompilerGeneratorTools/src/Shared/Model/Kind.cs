using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
public class Kind : IEquatable<Kind>
{
    [XmlAttribute]
    public string Name;

    public override bool Equals(object? obj)
        => Equals(obj as Kind);

    public bool Equals(Kind? other)
        => Name == other?.Name;

    public override int GetHashCode()
        => Name is null ? 0 : Name.GetHashCode();
}
#pragma warning restore CS8618
