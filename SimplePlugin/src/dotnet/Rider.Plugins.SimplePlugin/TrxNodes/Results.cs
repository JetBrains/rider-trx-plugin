using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rider.Plugins.SimplePlugin.TrxNodes;

public class Results
{
    [XmlElement("UnitTestResult")]
    public List<UnitTestResult> UnitTestResults { get; set; }
}
