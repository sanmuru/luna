using System.Xml;
using System.Xml.Serialization;

namespace Luna.Compilers.Generators.Model;

#pragma warning disable CS8618
public class Comment
{
    [XmlAnyElement]
    public XmlElement[] Body;
}
#pragma warning restore CS8618
