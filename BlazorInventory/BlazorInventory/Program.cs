using System.Diagnostics;
using System.Reflection;
using ActualLab.Fusion;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorInventory.Components.Account;
using BlazorInventory.Data;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Npgsql;
using ActualLab.Fusion.Extensions;
using ActualLab.Fusion.Server;
using ActualLab.Rpc;
using ActualLab.Rpc.Server;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Client;
using BlazorInventory.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
builder.Services.AddRazorComponents(options =>
    {
        {
            if (builder.Environment.IsDevelopment())
                options.DetailedErrors = true;
        }
    })
    .AddInteractiveServerComponents(options =>
    {
        if (builder.Environment.IsDevelopment())
            options.DetailedErrors = true;
    })
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(static options => options.SerializeAllClaims = true);

builder.Services.Configure<ForwardedHeadersOptions>(static options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(static options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme; //IdentityConstants.ExternalScheme;
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

builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), static npgsql => npgsql.EnableRetryOnFailure(0));

    options.UseNpgsqlHintFormatter();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
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

builder.Services.AddIdentityCore<ApplicationUser>(static options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

ClientStartup.ConfigureSharedServices(builder.Services);

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

var app = builder.Build();

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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
});

app.UseHttpsRedirection();

app.UseHealthChecks("/healthz");
app.MapPrometheusScrapingEndpoint()
    .AllowAnonymous();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});
app.UseFusionSession();
app.UseRouting();

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

await using var db = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync().ConfigureAwait(false);
await db.Database.MigrateAsync().ConfigureAwait(false);

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
    // Fusion
    var fusion = builder.Services.AddFusion(RpcServiceMode.Server);
    fusion.AddWebServer();

    // Fusion services
    fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use

    fusion.AddService<IItemService, ItemService>();
    fusion.AddService<ILabelService, LabelService>();
    fusion.AddService<IForeignServerService, ForeignServerService>();
    fusion.AddService<IScanService, ScanService>();

    fusion.AddBlazor();
    fusion.AddOperationReprocessor();
    builder.Services.AddBlazorCircuitActivitySuppressor();
}
