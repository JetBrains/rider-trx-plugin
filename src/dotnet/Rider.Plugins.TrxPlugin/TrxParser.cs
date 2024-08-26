namespace Rider.Plugins.TrxPlugin;

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using TrxNodes;
using UnitTestResult = TrxNodes.UnitTestResult;

public static class TrxParser
{
     private static String ErrorCodeToStatus(int code)
    {
        if (code > 5 && code != 10) return "NotSupported";
        return new List<string> { "Passed", "Failed", "Inconclusive", "Timeout", "Aborted", "NotExecuted" }[
            code % 10];
    }

    public static List<UnitTestResult> ParseResults(XElement node, XNamespace defaultNamespace)
    {
        var results = new List<UnitTestResult>();
        foreach (var result in node.Elements())
        {
            if (result.Name.LocalName == "UnitTestResult")
            {
                UnitTestResult unitTestResult = null;
                if (defaultNamespace.ToString() == "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
                {
                    var serializer = new XmlSerializer(typeof(UnitTestResult),
                        defaultNamespace.ToString());
                    var startNode = new XElement(result);
                    using var reader = startNode.CreateReader();
                    unitTestResult = (UnitTestResult)serializer.Deserialize(reader);
                    if (unitTestResult == null)
                    {
                        continue;
                    }
                }
                else
                {
                    unitTestResult = new UnitTestResult()
                    {
                        TestId = result.Element("id")?.Element("testId")?.Element("id")?.Value,
                        TestName = result.Element("testName")?.Value,
                        Outcome = ErrorCodeToStatus(int.Parse(result.Element("outcome")?.Element("value__")?.Value??"0")),
                        Duration = result.Element("duration")?.Value,
                        Output = new Output()
                        {
                            ErrorInfo = new ErrorInfo()
                            {
                                Message = result.Element("errorInfo")?.Element("message")?.Value,
                                StackTrace = result.Element("errorInfo")?.Element("stackTrace")?.Value
                            }
                        }
                    };
                }

                results.Add(unitTestResult);
            }
            else
            {
                results.AddRange(ParseResults(result, defaultNamespace));
            }
        }

        return results;
    }

    public static void AddDefinitions(XElement node, List<UnitTestResult> results, XNamespace defaultNamespace)
    {
        foreach (var element in node.Elements())
        {
            if (defaultNamespace.ToString() == "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
            {
                if (element.Name.LocalName == "UnitTest")
                {
                    var serializer = new XmlSerializer(typeof(UnitTest),
                        "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                    var startNode = new XElement(element);
                    using var reader = startNode.CreateReader();
                    var unitTest = (UnitTest)serializer.Deserialize(reader);
                    foreach (var result in results)
                    {
                        if (result.TestId == unitTest?.Id)
                        {
                            result.Definition = unitTest;
                        }
                    }
                }
                else
                {
                    AddDefinitions(element, results, defaultNamespace);
                }
            }
            else
            {
                if (element.Name.LocalName == "tests")
                {
                    foreach (var test in element.Elements("value"))
                    {
                        var testId = test.Element("id")?.Element("id")?.Value;
                        var className = test.Element("testMethod")?.Element("className")?.Value;
                        foreach (var result in results)
                        {
                            if (result.TestId == testId)
                            {
                                result.Definition = new UnitTest()
                                {
                                    TestMethod = new TestMethod()
                                    {
                                        ClassName = className
                                    },
                                    Id = testId
                                };
                            }
                        }
                    }
                }
                else
                {
                    AddDefinitions(element, results, defaultNamespace);
                }
            }
        }
    }

    public static void AddInnerResults(UnitTestResult result, List<UnitTestResult> results)
    {
        if (result.InnerResults == null)
        {
            return;
        }

        foreach (var innerResult in result.InnerResults.UnitTestResults)
        {
            results.Add(innerResult);
            AddInnerResults(innerResult, results);
        }
    }
}
