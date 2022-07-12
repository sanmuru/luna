using System.Xml;
using System.Xml.Serialization;

namespace SyntaxGenerator;

public class Comment
{
    [XmlAnyElement]
    public XmlElement[] Body;
}
