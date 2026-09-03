using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;

namespace BlazorInventory.Client;

/// <summary>
/// <see cref="ComputedStateComponent{T}"/> variant that waits for the initial
/// compute to produce a value (or error) before the component's first render.
/// Keeps the data in the initial (prerendered) HTML and prevents the
/// blank-then-data flash on fresh page loads.
/// </summary>
public abstract class AwaitedComputedStateComponent<T> : ComputedStateComponent<T>
{
    protected override async Task OnInitializedAsync()
    {
        EnsureStateIsCreated();
        await State.WhenSynchronized(ComputedSynchronizer.Default);
    }
}
