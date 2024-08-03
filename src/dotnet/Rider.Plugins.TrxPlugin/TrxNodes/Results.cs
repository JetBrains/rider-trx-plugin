using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rider.Plugins.TrxPlugin.TrxNodes;
public class Results
{
    [XmlElement("UnitTestResult")] public List<UnitTestResult> UnitTestResults { get; set; }
}
