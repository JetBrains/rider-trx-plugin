using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ProjectModel;

namespace Rider.Plugins.SimplePlugin;

[ZoneMarker]
public class ZoneMarker : IRequire<IProjectModelZone>;
