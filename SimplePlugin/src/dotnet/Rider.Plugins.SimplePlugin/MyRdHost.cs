using System.Threading.Tasks;
using System.Xml;
using System;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using Rider.Plugins.SimplePlugin.Model;

namespace Rider.Plugins.SimplePlugin;

[SolutionComponent]
public class MyRdHost
{
    public MyRdHost(ISolution solution)
    {
        var model = solution.GetProtocolSolution().GetRdSimplePluginModel();
        model.MyCall.SetAsync(HandleCall);
    }

    private async Task<RdCallResponse> HandleCall(Lifetime lt, RdCallRequest request)
    {
        string path = request.MyField;

        await Task.Delay(1000, lt);
        return lt.Execute(() => new RdCallResponse(path));
    }
}
