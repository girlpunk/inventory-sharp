using ActualLab.Fusion.EntityFramework;
using ActualLab.IO;
using InventorySharp.Models.Geolocation;
using Microsoft.EntityFrameworkCore;

namespace InventorySharp;

using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Extensions;
using ActualLab.Fusion.UI;
using Components;
//using Components.Pages;
using FluentValidation;
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
            .AddInteractiveServerComponents();

        builder.Services.AddScoped<IGeolocationService, GeolocationService>();

        builder.Services.AddPooledDbContextFactory<AppDbContext>(db =>
        {
            // if (AppSettings.Db.UsePostgreSql) {
            //     var connectionString =
            //         "Server=localhost;Database=fusion_hellocart;Port=5432;User Id=postgres;Password=postgres";
            //     db.UseNpgsql(connectionString, npgsql => {
            //         npgsql.EnableRetryOnFailure(0);
            //     });
            //     db.UseNpgsqlHintFormatter();
            // }
            // else {
                var appTempDir = FilePath.GetApplicationTempDirectory("", true);
                var dbPath = appTempDir & "HelloCart_v1.db";
                db.UseSqlite($"Data Source={dbPath}");
            // }
            db.EnableSensitiveDataLogging();
        });

        builder.Services.AddDbContextServices<AppDbContext>(db =>
        {
            db.AddOperations(operations =>
            {
                // if (AppSettings.Db.UsePostgreSql)
                    // operations.AddNpgsqlOperationLogWatcher();
                // else
                    operations.AddFileSystemOperationLogWatcher();
            });
            db.AddEntityResolver<Guid, Item>();
            // db.AddEntityResolver<string, DbCart>(_ => new() {
            //     // Cart is always loaded together with items
            //     QueryTransformer = carts => carts.Include(c => c.Items),
            // });
        });

        // Operation reprocessor
        fusionBuilder.AddOperationReprocessor();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.UseSession();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            // .AddInteractiveWebAssemblyRenderMode()
            .AddInteractiveServerRenderMode();

        //app.UseAuthorization();

        await (await app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync()).Database.MigrateAsync();

        await app.RunAsync();
    }
}
