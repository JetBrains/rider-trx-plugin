using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.Util;

namespace Rider.Plugins.TrxPlugin.TransientTestSessions
{
  public class TestElement : UnitTestElement
  {
    [Persist, NotNull] public string DisplayName { get; set; }
    [Persist(typeof(UnitTestElementNamespace.Marshaller))]
    public UnitTestElementNamespace Namespace { get; set; }

    public override string ShortName => DisplayName;
    public override string Kind => "Transient";

    [UsedImplicitly]
    public TestElement() { }

    public TestElement(
      [NotNull] string displayName,
      [CanBeNull] UnitTestElementNamespace ns)
    {
      DisplayName = displayName;
      Namespace = ns;
    }

    public override IDeclaredElement GetDeclaredElement() => null;
    public override IReadOnlyCollection<IUnitTestElement> GetRelatedUnitTestElements() => EmptyArray<IUnitTestElement>.Instance;
    public override IEnumerable<UnitTestElementLocation> GetLocations() => EmptyArray<UnitTestElementLocation>.Instance;
    public override IEnumerable<IProjectFile> GetProjectFiles() => EmptyArray<IProjectFile>.Instance;
    public override UnitTestElementNamespace GetNamespace() => Namespace ?? UnitTestElementNamespace.Global;
  }
}
