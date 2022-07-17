using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

public class TreeTypeChild
{

    [XmlAttribute]
    public string Optional;
    public bool IsOptional => string.Equals(Optional, "true", StringComparison.InvariantCultureIgnoreCase);
}

public class Choice : TreeTypeChild
{
    // Note: 'Choice's should not be children of a 'Choice'.  It's not necessary, and the child
    // choice can just be inlined into the parent.
    [XmlElement(ElementName = "Field", Type = typeof(Field))]
    [XmlElement(ElementName = "Sequence", Type = typeof(Sequence))]
    public List<TreeTypeChild> Children;
}

public class Sequence : TreeTypeChild
{
    // Note: 'Sequence's should not be children of a 'Sequence'.  It's not necessary, and the
    // child choice can just be inlined into the parent.
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
    public string Override;

    [XmlAttribute]
    public string New;

    [XmlAttribute]
    public int MinCount;

    [XmlAttribute]
    public bool AllowTrailingSeparator;

    [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
    public List<Kind> Kinds = new();

    [XmlElement]
    public Comment PropertyComment;

    public bool IsToken => Type == "SyntaxToken";
}
