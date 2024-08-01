using System.Xml.Serialization;

namespace Rider.Plugins.SimplePlugin.TrxNodes;

public class UnitTestResult
{
    public UnitTest Definition { get; set; }

    [XmlAttribute("executionId")] public string Id { get; set; }

    [XmlAttribute("parentExecutionId")] public string ParentId { get; set; }

    [XmlAttribute("testId")] public string TestId { get; set; }

    [XmlAttribute("testListId")] public string TestListId { get; set; }

    [XmlAttribute("testName")] public string TestName { get; set; }

    [XmlAttribute("computerName")] public string ComputerName { get; set; }

    [XmlAttribute("duration")] public string Duration { get; set; }

    [XmlAttribute("startTime")] public string StartTime { get; set; }

    [XmlAttribute("endTime")] public string EndTime { get; set; }

    [XmlAttribute("testType")] public string TestTypeId { get; set; }

    [XmlAttribute("relativeResultsDirectory")]
    public string RelativeResultsDirectoryId { get; set; }

    [XmlAttribute("outcome")] public string Outcome { get; set; }

    [XmlAttribute("resultType")] public string ResultType { get; set; }

    [XmlAttribute("dataRowInfo")] public string DataRowInfo { get; set; }

    [XmlElement("Output")] public Output Output { get; set; }

    [XmlElement("InnerResults")] public Results InnerResults { get; set; }
}
