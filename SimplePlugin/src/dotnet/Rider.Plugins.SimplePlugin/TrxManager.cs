using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using Rider.Plugins.SimplePlugin.Model;
using JetBrains.ReSharper.UnitTestFramework.Execution;
using JetBrains.ReSharper.UnitTestFramework.Session;
using JetBrains.ReSharper.UnitTestFramework.UI.Session;
using Rider.Plugins.SimplePlugin.TrxNodes;
using System.Xml.Serialization;
using UnitTestResult = Rider.Plugins.SimplePlugin.TrxNodes.UnitTestResult;

namespace Rider.Plugins.SimplePlugin;

[SolutionComponent]
class TrxManager
{
    private readonly IUnitTestSessionRepository myRepository;
    private readonly IUnitTestSessionConductor mySessionConductor;
    private readonly IUnitTestResultManager myResultManager;
    private readonly RdSimplePluginModel myModel;
    private string output = "";
    public TrxManager(Lifetime lifetime, IUnitTestSessionRepository repository, IUnitTestSessionConductor sessionConductor,
        IUnitTestResultManager resultManager, ISolution solution)
    {
        myRepository = repository;
        mySessionConductor = sessionConductor;
        myResultManager = resultManager;
        myModel = solution.GetProtocolSolution().GetRdSimplePluginModel();
        myModel.MyCall.SetAsync(HandleCall);
    }

    List<UnitTestResult> ParseResults(XElement node, Dictionary<string, string> namespaces)
    {
        var results = new List<UnitTestResult>();
        foreach (var ns in node.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            var prefix = ns.Name.LocalName == "xmlns" ? "" : ns.Name.LocalName;
            var namespaceUri = ns.Value;
            namespaces[prefix] = namespaceUri;
        }

        foreach (var result in node.Elements())
        {
            if (result.Name.LocalName == "UnitTestResult")
            {
                var serializer = new XmlSerializer(typeof(UnitTestResult), namespaces[""]);
                var startNode = new XElement(result);
                foreach (var ns in namespaces)
                {
                    XNamespace xns = XNamespace.Get(ns.Value);
                    string prefix = ns.Key;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        startNode.SetAttributeValue(XNamespace.Xmlns + prefix, xns);
                    }
                }
                using (var reader = startNode.CreateReader())
                {
                    var unitTestResult = (UnitTestResult)serializer.Deserialize(reader);
                    results.Add(unitTestResult);
                }
            }
            else
            {
                results.AddRange(ParseResults(result, namespaces));
            }
        }
        return results;
    }

    private async Task<bool> Do(string trxFilePath)
    {
        XDocument document;
        await using (var stream = File.OpenRead(trxFilePath))
        {
            document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        }
        var root = document.Root;
        if (root == null)
        {
            return false;
        }
        var results = ParseResults(root, new Dictionary<string, string>());

        /*
        myResultManager.TestFinishing();
        myResultManager.TestException();
        myRepository.Query(new TestIdCriterion());

        var unitTestSession = myRepository.CreateSession(new TestElementCriterion());

        mySessionConductor.OpenSession(unitTestSession);
        */

        return true;
    }
    private async Task<RdCallResponse> HandleCall(Lifetime lt, RdCallRequest request)
    {
        string path = request.MyField;
        if (await Do(path))
        {
            return lt.Execute(() => new RdCallResponse("Success"));
        }
        return lt.Execute(() => new RdCallResponse("Failed"));
    }
}
