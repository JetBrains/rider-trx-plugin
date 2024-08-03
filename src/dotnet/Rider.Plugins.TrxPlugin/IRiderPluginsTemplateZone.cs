using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ProjectModel;

namespace Rider.Plugins.TrxPlugin;

[ZoneMarker]
public class ZoneMarker : IRequire<IProjectModelZone>;
