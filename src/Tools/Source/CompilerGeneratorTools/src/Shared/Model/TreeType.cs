using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
public abstract class TreeType
{
    [XmlAttribute]
    public string Name;

    [XmlAttribute]
    public string? Base;

    [XmlAttribute]
    public string? SkipConvenienceFactories;

    [XmlElement]
    public Comment? TypeComment;

    [XmlElement]
    public Comment? FactoryComment;

    [XmlElement(ElementName = "Field", Type = typeof(Field))]
    [XmlElement(ElementName = "Choice", Type = typeof(Choice))]
    [XmlElement(ElementName = "Sequence", Type = typeof(Sequence))]
    public List<TreeTypeChild> Children;
}
#pragma warning restore CS8618
