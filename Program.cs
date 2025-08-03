using System.Diagnostics;
using System.Reflection;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Npgsql;
using InventorySharp.Models.Geolocation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InventorySharp;

using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using Components;
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
            })
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(static options => options.SerializeAllClaims = true);

        builder.Services.Configure<ForwardedHeadersOptions>(static options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
        });

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddAuthentication(static options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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

                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

        builder.Services.AddAuthorization();

        builder.Services.AddScoped<IGeolocationService, GeolocationService>();

        builder.Services.AddHttpContextAccessor();

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
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

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
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode();

        app.MapGroup("/authentication").MapLoginAndLogout();

        app.UseAuthorization();
        app.UseAuthentication();

        await using var db = await app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync().ConfigureAwait(false);
        await db.Database.MigrateAsync().ConfigureAwait(false);

        await app.RunAsync().ConfigureAwait(false);
    }

    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", static (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        group.MapPost("/logout", static ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }

    public static IServiceCollection ConfigureCookieOidc(this IServiceCollection services, string cookieScheme, string oidcScheme)
    {
        // services.AddSingleton<CookieOidcRefresher>();
        services.AddOptions<CookieAuthenticationOptions>(cookieScheme)/*.Configure<CookieOidcRefresher>((cookieOptions, refresher) =>
        {
            cookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, oidcScheme);
        })*/;
        services.AddOptions<OpenIdConnectOptions>(oidcScheme).Configure(oidcOptions =>
        {
            // Request a refresh_token.
            oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);
            // Store the refresh_token.
            oidcOptions.SaveTokens = true;
        });
        return services;
    }
}
