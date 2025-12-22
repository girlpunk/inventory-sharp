using System.Diagnostics.CodeAnalysis;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Blazor.Authentication;
using ActualLab.Fusion.Client.Interception;
using ActualLab.Fusion.Diagnostics;
using ActualLab.Fusion.Extensions;
using ActualLab.Fusion.UI;
using ActualLab.OS;
using ActualLab.Rpc;
using AntDesign;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Client.Geolocation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorInventory.Client;

[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCodeAttribute", Justification = "Fine here")]
public static class ClientStartup
{
    internal static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
    {
        // Logging
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Fusion
        var fusion = services.AddFusion();
        // fusion.AddAuthClient();
        fusion.AddBlazor()
            // .AddAuthentication()
            .AddPresenceReporter();

        var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
        fusion.Rpc.AddWebSocketClient(baseUri);

        // Fusion service clients
        fusion.AddClient<IItemService>();
        fusion.AddClient<ILabelService>();
        fusion.AddClient<IForeignServerService>();
        fusion.AddClient<IScanService>();
        fusion.AddClient<ITagService>();

        ConfigureSharedServices(services, HostKind.Client);
    }

    public static void ConfigureSharedServices(IServiceCollection services, HostKind hostKind)
    {
        // Other UI-related services
        var fusion = services.AddFusion();
        fusion.AddBlazor();
        fusion.AddFusionTime();

        // Default update delay is set to 0.1s
        // services.AddTransient<IUpdateDelayer>(static c => new UpdateDelayer(c.UIActionTracker(), 0.01));

        services.AddScoped<IGeolocationService, GeolocationService>();

        services.AddAntDesign();
        LocaleProvider.DefaultLanguage = "en-GB";

        if (hostKind != HostKind.BackendServer)
        {
            // Highly recommended option for client & API servers:
            // RpcWebSocketClientOptions.Default = new RpcWebSocketClientOptions() {
            //     UseAutoFrameDelayerFactory = true,
            // };

            // Lets ComputedState to be dependent on, e.g., current culture - use only if you need this:
            // ComputedState.DefaultOptions.FlowExecutionContext = true;
            // fusion.Rpc.AddWebSocketClient(remoteRpcHostUrl);
            if (hostKind is HostKind.ApiServer or HostKind.SingleServer)
                // {
                // All server-originating RPC connections should go to the default backend server
                RpcPeerRef.Default = RpcPeerRef.GetDefaultPeerRef(true);
                // And want to call the client via this server-side RPC client:
                //fusion.Rpc.AddClient<ISimpleClientSideService>();
            // }

            // If we're here, hostKind is Client, ApiServer, or SingleServer
            // fusion.AddService<Todos>(ServiceLifetime.Scoped);
            services.AddScoped(static c => new RpcPeerStateMonitor(c, OSInfo.IsAnyClient ? RpcPeerRef.Default : null));
            services.AddScoped<IUpdateDelayer>(static c => new UpdateDelayer(c.UIActionTracker(), 0.25)); // 0.25s

            // Uncomment to make computed state components to re-render only on re-computation of their state.
            // Click on DefaultOptions to see when they re-render by default.
            // ComputedStateComponent.DefaultOptions = ComputedStateComponentOptions.RecomputeStateOnParameterChange;
        }

        // Diagnostics
        if (hostKind == HostKind.Client)
            RpcPeer.DefaultCallLogLevel = LogLevel.Debug;

        services.AddHostedService(static c =>
        {
            var isWasm = OSInfo.IsWebAssembly;
            return new FusionMonitor(c)
            {
                SleepPeriod = TimeSpan.FromSeconds(isWasm ? 0 : 15),
                CollectPeriod = TimeSpan.FromSeconds(isWasm ? 5 : 15),
                AccessFilter = isWasm
                    ? static computed => computed.Input.Function is RemoteComputeMethodFunction
                    : static _ => true,
                AccessStatisticsPreprocessor = StatisticsPreprocessor,
                RegistrationStatisticsPreprocessor = StatisticsPreprocessor,
            };

            static void StatisticsPreprocessor(Dictionary<string, (int, int)> stats)
            {
                foreach (var key in stats.Keys.ToList())
                {
                    if (key.Contains(".Pseudo"))
                        stats.Remove(key);
                    if (key.StartsWith("FusionTime.", StringComparison.Ordinal))
                        stats.Remove(key);
                }
            }
        });
    }
}
