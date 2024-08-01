using System.Xml.Serialization;

namespace Rider.Plugins.SimplePlugin.TrxNodes;

public class Execution
{
    [XmlAttribute("id")] public string Id { get; set; }
}
