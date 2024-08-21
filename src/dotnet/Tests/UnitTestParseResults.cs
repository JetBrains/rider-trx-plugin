using System.Xml.Linq;
using JetBrains.Application.Components;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.UnitTestFramework.Caching;
using JetBrains.ReSharper.UnitTestFramework.Execution;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.ReSharper.UnitTestFramework.Session;
using JetBrains.ReSharper.UnitTestFramework.UI.Session;
using JetBrains.Rider.Model;
using Moq;
using Rider.Plugins.TrxPlugin;
using ILogger = JetBrains.Util.ILogger;


namespace Tests;

public class TestParseResults
{
    private TrxManager _trxManager;
    private int _passed;
    private int _failed;
    private int _warning;

    [SetUp]
    public void Setup()
    {
        _trxManager = new TrxManager(Lifetime.Eternal, null);
    }

    [Test]
    public void Test1()
    {
        XDocument document;
        using (var stream = File.OpenRead("../../../TestData/test1.trx"))
        {
            document = XDocument.Load(stream);
        }

        var root = document.Root;

        var results = _trxManager.ParseResults(root);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].TestName, Is.EqualTo("ParentTest"));
        Assert.That(results[0].Outcome, Is.EqualTo("Passed"));
        Assert.That(TimeSpan.Parse(results[0].Duration), Is.EqualTo(TimeSpan.FromDays(1)));
        Assert.That(results[0].Output.ErrorInfo, Is.Null);
        Assert.That(results[0].InnerResults.UnitTestResults[0].TestName, Is.EqualTo("ChildTest1"));
        Assert.That(results[0].InnerResults.UnitTestResults[1].TestName, Is.EqualTo("ChildTest2"));
    }

    [Test]
    public void Test2()
    {
        XDocument document;
        using (var stream = File.OpenRead("../../../TestData/test2.trx"))
        {
            document = XDocument.Load(stream);
        }

        var root = document.Root;

        var results = _trxManager.ParseResults(root);
        Assert.That(results.Count, Is.EqualTo(20));
        foreach (var result in results)
        {
            if (result.Outcome.ToLower() == "passed")
            {
                _passed += 1;
            }

            if (result.Outcome.ToLower() == "failed")
            {
                _failed += 1;
            }

            if (result.Outcome.ToLower() == "warn")
            {
                _warning += 1;
            }
        }

        Assert.That(_passed, Is.EqualTo(6));
        Assert.That(_failed, Is.EqualTo(13));
        Assert.That(_warning, Is.EqualTo(1));
    }

    [Test]
    public void Test3()
    {
        XDocument document;
        using (var stream = File.OpenRead("../../../TestData/test3.trx"))
        {
            document = XDocument.Load(stream);
        }

        var root = document.Root;

        var results = _trxManager.ParseResults(root);
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results[1].TestName, Is.EqualTo("IndependentTest"));
        Assert.That(results[1].Outcome, Is.EqualTo("Passed"));
        Assert.That(results[0].TestName, Is.EqualTo("MainTest"));
        Assert.That(results[0].Outcome, Is.EqualTo("Failed"));
        Assert.That(results[0].InnerResults?.UnitTestResults?.Count, Is.EqualTo(2));
        Assert.That(results[0].InnerResults.UnitTestResults[0].TestName, Is.EqualTo("SubTest1"));
        Assert.That(results[0].InnerResults.UnitTestResults[1].TestName, Is.EqualTo("SubTest2"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].Outcome, Is.EqualTo("Passed"));
        Assert.That(results[0].InnerResults.UnitTestResults[1].Outcome, Is.EqualTo("Failed"));
    }

    [Test]
    public void Test4()
    {
        XDocument document;
        using (var stream = File.OpenRead("../../../TestData/test4.trx"))
        {
            document = XDocument.Load(stream);
        }

        var root = document.Root;

        var results = _trxManager.ParseResults(root);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].TestName, Is.EqualTo("MainTest"));
        Assert.That(results[0].Outcome, Is.EqualTo("Passed"));
        Assert.That(results[0].InnerResults?.UnitTestResults?.Count, Is.EqualTo(2));
        Assert.That(results[0].InnerResults.UnitTestResults[0].TestName, Is.EqualTo("SubTest1"));
        Assert.That(results[0].InnerResults.UnitTestResults[1].TestName, Is.EqualTo("SubTest2"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].Outcome, Is.EqualTo("Failed"));
        Assert.That(results[0].InnerResults.UnitTestResults[1].Outcome, Is.EqualTo("Failed"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].InnerResults?.UnitTestResults?.Count, Is.EqualTo(2));
        Assert.That(results[0].InnerResults.UnitTestResults[0].InnerResults.UnitTestResults[0].TestName,
            Is.EqualTo("SubSubTest1"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].InnerResults.UnitTestResults[1].TestName,
            Is.EqualTo("SubSubTest2"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].InnerResults.UnitTestResults[0].Outcome,
            Is.EqualTo("Failed"));
        Assert.That(results[0].InnerResults.UnitTestResults[0].InnerResults.UnitTestResults[1].Outcome,
            Is.EqualTo("Failed"));
    }
}
