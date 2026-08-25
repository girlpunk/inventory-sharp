using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Components;

public partial class Routes
{
    [Parameter] public string SessionId { get; set; } = "";
    [Parameter] public string RenderModeKey { get; set; } = "";

    [Inject] private ILogger<Routes> Logger { get; set; } = null!;

    protected override void OnInitialized()
    {
        CircuitHub.SessionResolver.Session = new Session(SessionId);

        if (CircuitHub.IsInteractive)
            CircuitHub.Initialize(this.GetDispatcher(), RenderModeDef.GetOrDefault(RenderModeKey));

        // TEMP DIAGNOSTIC: remove once the empty-list-on-first-load bug is resolved.
        Logger.LogWarning(
            "DIAG circuit initialized: session={SessionId}, renderModeKey={RenderModeKey}, interactive={IsInteractive}",
            SessionId, RenderModeKey, CircuitHub.IsInteractive);
    }
}
