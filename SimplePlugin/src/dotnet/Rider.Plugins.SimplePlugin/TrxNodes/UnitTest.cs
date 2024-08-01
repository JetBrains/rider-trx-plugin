using System.Xml.Serialization;

namespace Rider.Plugins.SimplePlugin.TrxNodes;

public class UnitTest
{
    [XmlAttribute("id")] public string Id { get; set; }

    [XmlAttribute("name")] public string Name { get; set; }

    [XmlAttribute("storage")] public string Storage { get; set; }

    [XmlElement("Execution")] public Execution Execution { get; set; }

    [XmlElement("TestMethod")] public TestMethod TestMethod { get; set; }
}
