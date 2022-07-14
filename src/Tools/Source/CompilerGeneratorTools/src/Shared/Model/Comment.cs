using System.Xml;
using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

public class Comment
{
    [XmlAnyElement]
    public XmlElement[] Body;
}
