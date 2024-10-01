using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.UnitTestFramework;

namespace Rider.Plugins.TrxPlugin;

[ZoneMarker]
public class ZoneMarker : IRequire<IUnitTestingZone>;
