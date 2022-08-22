using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
[XmlRoot]
public class Tree
{
    [XmlAttribute]
    public string Root;

    [XmlElement(ElementName = "Node", Type = typeof(Node))]
    [XmlElement(ElementName = "AbstractNode", Type = typeof(AbstractNode))]
    [XmlElement(ElementName = "PredefinedNode", Type = typeof(PredefinedNode))]
    public List<TreeType> Types;
}
#pragma warning restore CS8618
