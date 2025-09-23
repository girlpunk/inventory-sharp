using System.Diagnostics.CodeAnalysis;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Extensions;
using ActualLab.Fusion.UI;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Client.Geolocation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace BlazorInventory.Client;

[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCodeAttribute", Justification = "Fine here")]
public static class ClientStartup
{
    public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
    {
        // Logging
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Fusion
        var fusion = services.AddFusion();
        var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
        fusion.Rpc.AddWebSocketClient(baseUri);

        // Fusion service clients
        fusion.AddClient<IItemService>();
        fusion.AddClient<ILabelService>();
        fusion.AddClient<IForeignServerService>();
        fusion.AddClient<IScanService>();

        ConfigureSharedServices(services);
    }

    public static void ConfigureSharedServices(IServiceCollection services)
    {
        // Other UI-related services
        var fusion = services.AddFusion();
        fusion.AddBlazor();
        fusion.AddFusionTime();

        // Default update delay is set to 0.1s
        services.AddTransient<IUpdateDelayer>(static c => new UpdateDelayer(c.UIActionTracker(), 0.1));

        services.AddScoped<IGeolocationService, GeolocationService>();

        // Also adds AI crap, so done manually below
        // services.AddRadzenComponents();

        services.AddScoped<DialogService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<TooltipService>();
        services.AddScoped<ContextMenuService>();
        services.AddScoped<ThemeService>();

        services.AddRadzenCookieThemeService(static options =>
        {
            options.Name = "InventorySharpTheme"; // The name of the cookie
            options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
        });
    }
}