using ActualLab.CommandR;
using ActualLab.DependencyInjection;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.OS;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client;

public partial class App
{
    private ISessionResolver SessionResolver => CircuitHub.SessionResolver;
    [Parameter] public string SessionId { get; set; } = "";
    [Parameter] public string RenderModeKey { get; set; } = "";

    protected override void OnInitialized()
    {
        if (OSInfo.IsWebAssembly)
        {
            // RPC auto-substitutes Session.Default to the cookie-based one on the server side for every call
            SessionResolver.Session = Session.Default;
            // That's how WASM app starts hosted services
            var rootServices = Services.Commander().Services;
            _ = rootServices.HostedServices().Start();
        }
        else
        {
            SessionResolver.Session = new Session(SessionId);
        }

        if (CircuitHub.IsInteractive)
            CircuitHub.Initialize(this.GetDispatcher(), RenderModeDef.GetOrDefault(RenderModeKey));
    }
}