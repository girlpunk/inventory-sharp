using System.Diagnostics;
using System.Reflection;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Npgsql;
using InventorySharp.Models.Geolocation;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InventorySharp;

using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using Components;
//using Components.Pages;
using Models;
using Services;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            if (builder.Environment.IsDevelopment())
            {
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("ActualLab.Fusion.Operations", LogLevel.Information);
                logging.AddFilter("ActualLab.Fusion.EntityFramework.Operations", LogLevel.Information);
                logging.AddFilter("ActualLab.Fusion.EntityFramework.Operations.LogProcessing", LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Connection", LogLevel.Warning);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            }
        });

        var fusionBuilder = builder.Services.AddFusion();

        fusionBuilder.AddBlazor();
        // fusionBuilder.Rpc.AddWebSocketClient(baseUri);

        fusionBuilder.AddService<IItemService, ItemService>();
        fusionBuilder.AddService<ILabelService, LabelService>();
        fusionBuilder.AddService<IForeignServerService, ForeignServerService>();
        fusionBuilder.AddService<IScanService, ScanService>();

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                if (builder.Environment.IsDevelopment())
                    options.DetailedErrors = true;
            });

        builder.Services.Configure<ForwardedHeadersOptions>(static options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
        });

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddAuthentication(static options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddOpenIdConnect(
                OpenIdConnectDefaults.AuthenticationScheme,
                "Authentik", options =>
                {
                    builder.Configuration.GetSection("Auth").Bind(options);
                    options.Scope.Add("email");

                    options.CorrelationCookie.Name = "OIDC-Correlation";
                    options.NonceCookie.Name = "OIDC-Nonce";
                })
            .AddIdentityCookies();

        builder.Services.AddScoped<IGeolocationService, GeolocationService>();

        builder.Services.AddPooledDbContextFactory<AppDbContext>(db =>
        {
            db.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                static npgsql => npgsql.EnableRetryOnFailure(0));
            db.UseNpgsqlHintFormatter();

            if (builder.Environment.IsDevelopment())
                db.EnableSensitiveDataLogging();
        });
        builder.Services.AddScoped(static sp =>
            sp.GetRequiredService<PooledDbContextFactory<AppDbContext>>().CreateDbContext());

        builder.Services.AddDbContextServices<AppDbContext>(static db =>
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

        // Operation reprocessor
        fusionBuilder.AddOperationReprocessor();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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
            .AddDbContextCheck<AppDbContext>("DB");

        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOutputCache();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            IdentityModelEventSource.ShowPII = true;
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
        }

        app.UseForwardedHeaders();

        app.UseHealthChecks("/healthz");
        app.MapPrometheusScrapingEndpoint()
            .AllowAnonymous();

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.UseSession();
        app.UseOutputCache();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            // .AddInteractiveWebAssemblyRenderMode()
            .AddInteractiveServerRenderMode();

        app.UseAuthorization();
        app.UseAuthentication();

        await using var db = await app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync().ConfigureAwait(false);
        await db.Database.MigrateAsync().ConfigureAwait(false);

        await app.RunAsync().ConfigureAwait(false);
    }
}
