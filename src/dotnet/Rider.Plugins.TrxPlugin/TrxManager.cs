using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using Rider.Plugins.TrxPlugin.Model;
using JetBrains.ReSharper.UnitTestFramework.Execution;
using JetBrains.ReSharper.UnitTestFramework.Session;
using JetBrains.ReSharper.UnitTestFramework.UI.Session;
using System.Xml.Serialization;
using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Caching;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.ReSharper.UnitTestFramework.UI.ViewModels;
using JetBrains.Util;
using JetBrains.Util.Dotnet.TargetFrameworkIds;
using JetBrains.Util.Logging;
using Rider.Plugins.TrxPlugin.TransientTestSessions;
using Rider.Plugins.TrxPlugin.TrxNodes;
using UnitTestResult = Rider.Plugins.TrxPlugin.TrxNodes.UnitTestResult;

namespace Rider.Plugins.TrxPlugin;

[SolutionComponent(Instantiation.ContainerAsyncPrimaryThread)]
public class TrxManager
{
    private readonly Lifetime _myLifetime;
    [NotNull] private readonly TestProvider _myTestProvider;
    [NotNull] private readonly IUnitTestElementRepository _myElementRepository;
    [NotNull] private readonly IUnitTestSessionRepository _mySessionRepository;
    [NotNull] private readonly IUnitTestSessionConductor _mySessionConductor;
    [NotNull] private readonly IUnitTestingProjectCache _myProjectCache;
    [NotNull] private readonly ILogger _myLogger;
    [NotNull] private readonly IUnitTestResultManager _myResultManager;
    [NotNull] private readonly ISolution _mySolution;

    public TrxManager(
        Lifetime lifetime,
        IComponentContainer componentContainer
    )
    {
        _myLifetime = lifetime;
        _myTestProvider = new TestProvider();
        _myElementRepository = componentContainer?.GetComponent<IUnitTestElementRepository>();
        _mySessionRepository = componentContainer?.GetComponent<IUnitTestSessionRepository>();
        _mySessionConductor = componentContainer?.GetComponent<IUnitTestSessionConductor>();
        _myResultManager = componentContainer?.GetComponent<IUnitTestResultManager>();
        _myProjectCache = componentContainer?.GetComponent<IUnitTestingProjectCache>();
        _myLogger = componentContainer?.GetComponent<ILogger>() ?? Logger.GetLogger<TrxManager>();
        _mySolution = componentContainer?.GetComponent<ISolution>();
        var myModel = _mySolution?.GetProtocolSolution().GetRdTrxPluginModel();
        myModel?.ImportTrxCall.SetAsync(HandleCall);

        _myLifetime.OnTermination(CloseAllUnitTestSessions);
        _mySessionConductor?.SessionClosed.Advise(_myLifetime, OnSessionClosed);
    }

    private void OnSessionClosed(IUnitTestSessionTreeViewModel sessionTreeViewModel)
    {
        var session = sessionTreeViewModel.Session;
        if (session.Name.Value.Contains(".trx"))
        {
            _ = _myElementRepository.Remove(session.Elements);
        }
    }

    private void CloseAllUnitTestSessions()
    {
        var sessions = _mySessionConductor.Sessions;
        foreach (var sessionTreeViewModel in sessions)
        {
            _mySessionConductor.CloseSession(sessionTreeViewModel.Session);
        }

        _myElementRepository.Clear();
    }

    private String ErrorCodeToStatus(int code)
    {
        if (code > 5 && code != 10) return "NotSupported";
        return new List<string> { "Passed", "Failed", "Inconclusive", "Timeout", "Aborted", "NotExecuted" }[
            code % 10];
    }

    public List<UnitTestResult> ParseResults(XElement node, XNamespace defaultNamespace)
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

    private void AddDefinitions(XElement node, List<UnitTestResult> results, XNamespace defaultNamespace)
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

    private void AddInnerResults(UnitTestResult result, List<UnitTestResult> results)
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

    private IUnitTestElement TestElementCreator(UnitTestResult current, IUnitTestTransaction tx,
        HashSet<IUnitTestElement> elements, string testRunId)
    {
        UnitTestElementNamespace ns = current.Definition?.TestMethod?.ClassName == null
            ? UnitTestElementNamespace.Create("")
            : UnitTestElementNamespace.Create(current.Definition.TestMethod.ClassName);
        TestElement element = new TestElement(current.TestName, ns)
        {
            NaturalId = UT.CreateId(_myProjectCache.GetProject(_mySolution.SolutionDirectory.ToString()),
                TargetFrameworkId.Default,
                this._myTestProvider,
                testRunId + ns + current.TestName)
        };
        if (elements.Contains(element))
        {
            return null;
        }

        if (current.InnerResults != null)
        {
            foreach (var child in current.InnerResults.UnitTestResults)
            {
                var childElement = TestElementCreator(child, tx, elements, testRunId);
                if (childElement != null)
                {
                    childElement.Parent = element;
                    tx.Create(childElement);
                    elements.Add(childElement);
                }
            }
        }

        return element;
    }


    private async Task<RdCallResponse> HandleTrx(string trxFilePath)
    {
        XDocument document;
        try
        {
            await using var stream = File.OpenRead(trxFilePath);
            document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _myLogger.Error(ex);
            return new RdCallResponse("Failed", "import.messages.error.open");
        }

        var root = document.Root;
        var defaultNamespace = root?.GetDefaultNamespace();

        List<UnitTestResult> results;
        try
        {
            results = ParseResults(root, defaultNamespace);
        }
        catch (Exception ex)
        {
            _myLogger.Error(ex);
            return new RdCallResponse("Failed", "import.messages.error.parse");
        }
        _myLogger.Error(results.Count.ToString());
        var countOuterResults = results.Count;
        try
        {
            for (int i = 0; i < countOuterResults; i++)
            {
                AddInnerResults(results[i], results);
            }
        }
        catch (Exception ex)
        {
            _myLogger.Error(ex);
            return new RdCallResponse("Failed", "import.messages.error.innerResults");
        }

        try
        {
            AddDefinitions(root, results, defaultNamespace);
        }
        catch (Exception ex)
        {
            _myLogger.Error(ex);
            return new RdCallResponse("Failed", "import.messages.error.definitions");
        }

        try
        {
            var testRunId = root?.Attribute("id")?.Value;
            if (defaultNamespace?.ToString() == "")
            {
                testRunId = root.Element("TestRun")?.Element("id")?.Value;
            }

            await DisplayResults(_myLifetime, results, testRunId, trxFilePath);
        }
        catch (Exception ex)
        {
            _myLogger.Error(ex);
            return new RdCallResponse("Failed", "import.messages.error.displayResults");
        }

        return new RdCallResponse("Success", "Successfully parsed trx file");
    }

    private async Task DisplayResults(CancellationToken ct, List<UnitTestResult> results, string id, string trxFilePath)
    {
        var existingSession = _mySessionRepository.GetById(new Guid(id));
        if (existingSession != null)
        {
            await _mySessionConductor.CloseSession(existingSession);
        }

        IUnitTestSession
            session = this._mySessionRepository.CreateSession(NothingCriterion.Instance,
                Path.GetFileName(trxFilePath), new Guid(id));
        HashSet<IUnitTestElement> elements = [];
        IUnitTestTransactionCommitResult transactionCommitResult = await this._myElementRepository.BeginTransaction(
            (tx =>
            {
                foreach (var result in results)
                {
                    var outerElement = TestElementCreator(result, tx, elements, id);
                    if (outerElement != null)
                    {
                        tx.Create(outerElement);
                        elements.Add(outerElement);
                    }
                }
            }), ct);
        UT.Facade.Append(
                new TestElementCriterion(elements)).To
            .Session(session);
        foreach (var element in elements)
        {
            var result = results.FirstOrDefault(r =>
                (r.Definition?.TestMethod?.ClassName ?? "") == element.GetNamespace().ToString() &&
                r.TestName == element.ShortName);

            if (result == null)
            {
                continue;
            }

            switch (result.Outcome?.ToLower())
            {
                case null:
                    break;
                case "passed":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Success, null,
                        TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
                case "failed":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Failed,
                        result.Output?.ErrorInfo?.Message,
                        TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    var exceptions = new List<TestException>
                    {
                        new TestException(null, result.Output?.ErrorInfo?.Message,
                            result.Output?.ErrorInfo?.StackTrace)
                    };
                    _myResultManager.TestException(element, session, exceptions);
                    break;
                case "aborted":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Aborted,
                        null, TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
                case "running":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Running,
                        null, TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
                case "inconclusive":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Inconclusive, null,
                        TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
                case "pending":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Pending,
                        null, TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
                case "notexecuted":
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Ignored,
                        result.Output?.ErrorInfo?.Message);
                    break;
                default:
                    _myResultManager.TestFinishing(element, session, UnitTestStatus.Unknown,
                        null, TimeSpan.Parse(result.Duration ?? "0", CultureInfo.InvariantCulture));
                    break;
            }

            _myResultManager.TestOutput(element, session, result.Output?.StdOut, TestOutputType.STDOUT);
        }

        IUnitTestSessionTreeViewModel sessionTreeViewModel = await this._mySessionConductor.OpenSession(session);
        sessionTreeViewModel.Grouping.Value = new UnitTestingGroupingSelection(UnitTestSessionTreeGroupings
            .GetSessionProviders(_mySolution, session)
            .Where(p => p.Key == "Namespace").ToArray());
    }

    private async Task<RdCallResponse> HandleCall(Lifetime lt, RdCallRequest request)
    {
        string path = request.TrxPath;
        return await lt.Execute(() => HandleTrx(path));
    }
}
