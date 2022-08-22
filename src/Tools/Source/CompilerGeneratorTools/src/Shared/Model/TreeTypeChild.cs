using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
public abstract class TreeTypeChild { }

public class Choice : TreeTypeChild
{
    // Choice节点不应嵌套Choice子节点，如果必要，则应内联子节点。
    [XmlElement(ElementName = "Field", Type = typeof(Field))]
    [XmlElement(ElementName = "Sequence", Type = typeof(Sequence))]
    public List<TreeTypeChild> Children;
}

public class Sequence : TreeTypeChild
{
    // Sequence节点不应嵌套Sequence子节点，如果必要，则应内联子节点。
    [XmlElement(ElementName = "Field", Type = typeof(Field))]
    [XmlElement(ElementName = "Choice", Type = typeof(Choice))]
    public List<TreeTypeChild> Children;
}

public class Field : TreeTypeChild
{
    [XmlAttribute]
    public string Name;

    [XmlAttribute]
    public string Type;

    [XmlAttribute]
    public string? Override;

    [XmlAttribute]
    public string? New;

    [XmlAttribute]
    public string? Optional;

    [XmlAttribute]
    public int MinCount;

    [XmlAttribute]
    public bool AllowTrailingSeparator;

    [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
    public List<Kind> Kinds;

    [XmlElement]
    public Comment? PropertyComment;

    public bool IsToken => this.Type == "SyntaxToken";
}
#pragma warning restore CS8618
