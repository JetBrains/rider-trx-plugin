using System.Linq;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Persistence;
using JetBrains.Util;
using Rider.Plugins.TrxPlugin.TrxNodes;

namespace Rider.Plugins.TrxPlugin;

public class RealElementsFinder(ISolution solution)
{
    private readonly IUnitTestElementRepository _myElementRepository = solution.GetComponent<IUnitTestElementRepository>();
    public UnitTestElement FindRealElement(UnitTestResult result)
    {
        using (solution.Locks.UsingReadLock())
        {
            var project = solution.GetAllProjects().SingleItem(x => x.Name == TrxParser.GetProjectName(result));
            if (project is null)
            {
                return null;
            }

            string className = result.Definition?.TestMethod?.ClassName;
            if (className is null)
            {
                return null;
            }
            var candidates = project.GetSubItemsRecursively(TrxParser.GetOnlyClassName(className) + ".cs");
            IProjectItem testFile = null;
            foreach (var candidate in candidates)
            {
                string path = candidate.Location.FullPath;
                if (path.Length < 3)
                {
                    continue;
                }
                if (path.Substring(path.Length - 3 - className.Length, className.Length).Replace('\\', '.') == className)
                {
                    testFile = candidate;
                    break;
                }
            }
            if (testFile is null)
            {
                return null;
            }

            var elements = _myElementRepository.Query(new ProjectFileCriterion((IProjectFile)testFile));

            return elements.FirstOrDefault(e => e.ShortName == result.TestName) as UnitTestElement;
        }
    }
}
