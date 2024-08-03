using System.Xml.Serialization;

namespace Rider.Plugins.TrxPlugin.TrxNodes;

public class ErrorInfo
{
    [XmlElement("Message")] public string Message { get; set; }

    [XmlElement("StackTrace")] public string StackTrace { get; set; }
}
