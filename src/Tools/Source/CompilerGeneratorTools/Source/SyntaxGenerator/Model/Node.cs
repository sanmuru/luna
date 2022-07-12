using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator;

public class Node : TreeType
{
    [XmlAttribute]
    public string Root;

    [XmlAttribute]
    public string Errors;

    [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
    public List<Kind> Kinds = new List<Kind>();

    public readonly List<Field> Fields = new List<Field>();
}
