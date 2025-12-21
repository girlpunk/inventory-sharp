using ActualLab.DependencyInjection;
using BlazorInventory.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

ClientStartup.ConfigureServices(builder.Services, builder);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

//await builder.Build().RunAsync();


var host = builder.Build();
// Blazor host doesn't start IHostedService-s by default,
// so let's start them "manually" here
var serviceStart = host.Services.HostedServices().Start();

var hostStart = host.RunAsync();

await Task.WhenAll(serviceStart, hostStart);
