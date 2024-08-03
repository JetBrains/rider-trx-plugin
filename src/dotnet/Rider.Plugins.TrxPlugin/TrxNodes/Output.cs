using System.Xml.Serialization;

namespace Rider.Plugins.TrxPlugin.TrxNodes;
public class Output
{
    [XmlElement("StdOut")] public string StdOut { get; set; }

    [XmlElement("StdErr")] public string StdErr { get; set; }

    [XmlElement("ErrorInfo")] public ErrorInfo ErrorInfo { get; set; }
}
