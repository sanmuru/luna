using System.Collections.Immutable;
using System.Xml.Serialization;
using Luna.Compilers.Generators.Model;

namespace Luna.Compilers.Generators.Symbols.Model;

#pragma warning disable CS8618
public abstract class SymbolTreeType : ITreeType<ITreeTypeChild>
{
    [XmlAttribute]
    public string Name;

    [XmlAttribute]
    public string? Base;

    string ITreeType<ITreeTypeChild>.Name => this.Name;
    string? ITreeType<ITreeTypeChild>.Base => this.Base;
    ImmutableList<ITreeTypeChild> ITreeType<ITreeTypeChild>.Children => ImmutableList<ITreeTypeChild>.Empty;
}
#pragma warning restore CS8618
