using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Symbols.Model;

public sealed class Symbol : SymbolTreeType
{
    [XmlAttribute]
    public string? Implement;
}
