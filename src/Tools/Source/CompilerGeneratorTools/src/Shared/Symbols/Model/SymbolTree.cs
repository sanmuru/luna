using System.Collections.Immutable;
using System.Xml.Serialization;
using Luna.Compilers.Generators.Model;

namespace Luna.Compilers.Generators.Symbols.Model;

#pragma warning disable CS8618
[XmlRoot(ElementName = "Tree")]
internal class SymbolTree : ITree<SymbolTreeType, ITreeTypeChild>
{
    [XmlAttribute]
    public string Root;

    [XmlElement(ElementName = "Symbol", Type = typeof(Symbol))]
    [XmlElement(ElementName = "AbstractSymbol", Type = typeof(AbstractSymbol))]
    [XmlElement(ElementName = "PredefinedSymbol", Type = typeof(PredefinedSymbol))]
    public List<SymbolTreeType> Types;

    string ITree<SymbolTreeType, ITreeTypeChild>.Root => this.Root;
    ImmutableList<SymbolTreeType> ITree<SymbolTreeType, ITreeTypeChild>.Types => this.Types.ToImmutableList();
}
#pragma warning restore CS8618
