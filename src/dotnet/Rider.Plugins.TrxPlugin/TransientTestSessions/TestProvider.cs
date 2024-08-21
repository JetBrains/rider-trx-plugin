using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Execution;
using JetBrains.ReSharper.UnitTestFramework.Execution.Hosting;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.Util;
using JetBrains.Util.Dotnet.TargetFrameworkIds;

namespace Rider.Plugins.TrxPlugin.TransientTestSessions
{
  [UnitTestProvider]
  public class TestProvider : IUnitTestProvider
  {
    public string ID => nameof(TestProvider);
    public string Name => nameof(TestProvider);

    public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind) => false;
    public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind) => false;
    public bool IsSupported(IHostProvider hostProvider, IProject project, TargetFrameworkId targetFrameworkId) => false;
    public bool IsSupported(IProject project, TargetFrameworkId targetFrameworkId) => false;
    public bool SupportsResultEventsForParentOf(IUnitTestElement element) => false;

    public IUnitTestRunStrategy GetRunStrategy(IUnitTestElement element, IHostProvider hostProvider)
    {
      return DoNothingRunStrategy.Instance;
    }
  }
}
