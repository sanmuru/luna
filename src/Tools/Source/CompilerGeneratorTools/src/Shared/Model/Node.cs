using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
public class Node : TreeType
{
    [XmlAttribute]
    public string? Root;

    [XmlAttribute]
    public string? Errors;

    [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
    public List<Kind> Kinds;

    public readonly List<Field> Fields = new();
}
#pragma warning restore CS8618
