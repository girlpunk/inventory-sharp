using System.Diagnostics;
using System.Reflection;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Blazor.Authentication;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Npgsql;
using ActualLab.Fusion.Extensions;
using ActualLab.Fusion.Server;
using ActualLab.Fusion.Server.Authentication;
using ActualLab.Fusion.Server.Endpoints;
using ActualLab.Rpc;
using ActualLab.Rpc.Server;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Client;
using BlazorInventory.Components.Account;
using BlazorInventory.Data;
using BlazorInventory.Data.Models;
using BlazorInventory.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using IPAddress = System.Net.IPAddress;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging();

builder.WebHost.UseDefaultServiceProvider(static (ctx, options) =>
{
    if (ctx.HostingEnvironment.IsDevelopment())
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    }
});

ConfigureFusionServices();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddRazorComponents(options =>
    {
        if (builder.Environment.IsDevelopment())
            options.DetailedErrors = true;
    })
    .AddInteractiveServerComponents(options =>
    {
        if (builder.Environment.IsDevelopment())
            options.DetailedErrors = true;
    })
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(static options => options.SerializeAllClaims = true);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;

    var headerSettings = builder.Configuration.GetSection("ForwardedHeaders");

    foreach (var proxy in headerSettings.GetSection("proxies").Get<string[]>() ?? [])
    {
        Console.WriteLine($"Adding known proxy: {proxy}");
        options.KnownProxies.Add(IPAddress.Parse(proxy));
    }

    foreach (var network in headerSettings.GetSection("network").Get<string[]>() ?? [])
    {
        Console.WriteLine($"Adding known proxy network: {network}");
        var parts = network.Split('/');
        options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(parts[0]), int.Parse(parts[1])));
    }
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

builder.Services.AddAuthentication(static options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(
        OpenIdConnectDefaults.AuthenticationScheme,
        "Authentik", options =>
        {
            builder.Configuration.GetSection("Auth").Bind(options);
            options.Scope.Add("email");

            options.CorrelationCookie.Name = "OIDC-Correlation";
            options.NonceCookie.Name = "OIDC-Nonce";

            options.SignInScheme = IdentityConstants.ApplicationScheme;
        })
    .AddIdentityCookies();

builder.Services.AddSession();

builder.Services.AddAuthorization();

builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), static npgsql => npgsql.EnableRetryOnFailure(0));

    options.UseNpgsqlHintFormatter();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.ConfigureWarnings(static warnings =>
        {
            warnings.Log(RelationalEventId.PendingModelChangesWarning);
        });
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped(static sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

builder.Services.AddDbContextServices<ApplicationDbContext>(static db =>
{
    db.AddOperations(static operations =>
    {
        operations.AddNpgsqlOperationLogWatcher();
    });
    db.AddEntityResolver<Guid, Item>();
    // db.AddEntityResolver<string, DbCart>(_ => new() {
    //     // Cart is always loaded together with items
    //     QueryTransformer = carts => carts.Include(c => c.Items),
    // });
});

builder.Services.AddMapster();

builder.Services.AddIdentityCore<ApplicationUser>(static options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Logging.AddOpenTelemetry(static logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(static r =>
    {
        r.AddService("Inventory-Sharp",
            serviceVersion: FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
            serviceInstanceId: Environment.MachineName);
        r.AddContainerDetector()
            .AddEnvironmentVariableDetector()
            .AddHostDetector()
            .AddOperatingSystemDetector()
            .AddProcessDetector()
            .AddProcessRuntimeDetector()
            .AddTelemetrySdk();
    })
    .WithMetrics(static (metrics) =>
    {
        metrics.AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddAspNetCoreInstrumentation()
            // .AddHttpClientInstrumentation()
            .AddNpgsqlInstrumentation()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        if (builder.Environment.IsDevelopment())
            tracing.SetSampler(new AlwaysOnSampler());

        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql();
        // .AddSource(DiagnosticHeaders.DefaultListenerName);
    });

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("DB");

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOutputCache();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

app.UsePathBase(app.Configuration.GetValue<string>("base_path", "/"));

StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseHealthChecks("/healthz");
app.MapPrometheusScrapingEndpoint()
    .AllowAnonymous();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});
app.UseFusionSession();
app.UseBlazorFrameworkFiles();

app.UseRouting();
app.MapRpcWebSocketServer();

app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

app.MapStaticAssets();
app.MapRazorComponents<BlazorInventory.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();


app.MapRpcWebSocketServer();
app.MapFusionRenderModeEndpoints();

await using var db = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
await db.Database.MigrateAsync();

app.Run();
return;


void ConfigureLogging()
{
    // Logging
    builder.Services.AddLogging(logging =>
    {
        // Use appsettings.*.json to change log filters
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
        if (builder.Environment.IsDevelopment())
        {
            logging.AddFilter("Microsoft", LogLevel.Warning);
            logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
            logging.AddFilter("ActualLab.Fusion.Operations", LogLevel.Information);
        }
    });
}

void ConfigureFusionServices()
{
    const HostKind hostKind = HostKind.SingleServer;

    // Fusion
    var fusion = builder.Services.AddFusion(RpcServiceMode.Server, true);
    var fusionServer = fusion.AddWebServer();

    fusionServer.ConfigureAuthEndpoint(static _ => new AuthEndpoints.Options
    {
        DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme,
        SignInPropertiesBuilder = static (_, properties) =>
        {
            properties.IsPersistent = true;
        }
    });
    fusionServer.ConfigureServerAuthHelper(static _ => new ServerAuthHelper.Options
    {
        NameClaimKeys = [],
    });

    fusionServer.AddMvc().AddControllers();
    fusion.AddOperationReprocessor();
    // fusion.AddDbAuthService<ApplicationDbContext, DbAuthSessionInfo, ApplicationUser, string>();

    // if (hostKind == HostKind.ApiServer) {
    //     fusion.AddClient<IAuth>(); // IAuth = a client of backend's IAuth
    //     fusion.AddClient<IAuthBackend>(); // IAuthBackend = a client of backend's IAuthBackend
    //     fusion.Rpc.Configure<IAuth>().IsServer(typeof(IAuth)).HasClient(); // Expose IAuth (a client) via RPC
    // }
    // else
    // {
    //     // SingleServer or BackendServer
    //     fusion.AddOperationReprocessor();
    //     fusion.AddDbAuthService<ApplicationDbContext, string>();
    //     if (hostKind == HostKind.BackendServer)
    //         fusion.Rpc.Configure<IAuthBackend>().IsServer(typeof(IAuthBackend)); // Expose IAuthBackend via RPC
    // }

    // Fusion services
    fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use

    AddService<IItemService, ItemService>(fusion, hostKind);
    AddService<ILabelService, LabelService>(fusion, hostKind);
    AddService<IForeignServerService, ForeignServerService>(fusion, hostKind);
    AddService<IScanService, ScanService>(fusion, hostKind);
    AddService<ITagService, TagService>(fusion, hostKind);

    fusion.AddBlazor()
        // .AddAuthentication()
        .AddPresenceReporter();
    fusion.AddOperationReprocessor();
    builder.Services.AddBlazorCircuitActivitySuppressor();

    ClientStartup.ConfigureSharedServices(builder.Services, hostKind);
}

void AddService<TInterface, TImplementation>(FusionBuilder fusion, HostKind hostKind) where TInterface : class, IComputeService where TImplementation : class, TInterface
{
    _ = hostKind switch
    {
        HostKind.SingleServer => fusion.AddService<TInterface, TImplementation>(),
        HostKind.BackendServer => fusion.AddServer<TInterface, TImplementation>(),
        HostKind.ApiServer => fusion.AddClient<TInterface>(),
        _ => throw new InvalidOperationException("Invalid host kind."),
    };
}