using System.Xml.Serialization;

namespace Rider.Plugins.TrxPlugin.TrxNodes;
public class Execution
{
    [XmlAttribute("id")] public string Id { get; set; }
}
