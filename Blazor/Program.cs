using System;
using System.Net.Http;
using System.Globalization;
using Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;

namespace Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        IConfiguration config = builder.Configuration;

        string apiEndpoint;
        if (builder.HostEnvironment.Environment == "Development")
        {
            apiEndpoint = "http://localhost:5001/";
        }
        else
        {
            apiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT")
                          ?? "https://karambit-api.mercantec.tech/"; // Production endpoint
        }

        Console.WriteLine($"API Endpoint: {apiEndpoint}");

        // Registers HttpClient to API service with a configurable endpoint
        builder.Services.AddHttpClient<APIService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
            Console.WriteLine($"APIService BaseAddress: {client.BaseAddress}");
        });
        builder.Services.AddScoped<BookingService>(sp => new BookingService(config));
        builder.Services.AddScoped<StorageService>(sp =>
            new StorageService(sp.GetRequiredService<IJSRuntime>()));
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<StorageService>(sp =>
            new StorageService(sp.GetRequiredService<IJSRuntime>()));
        builder.Services.AddScoped<ClientJwtService>();

        // Configure HttpClient
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });
        builder.Services.AddScoped(sp => new HubConnectionBuilder()
            .WithUrl("http://localhost:5001/signalrhub") // Match API port
            .Build());

        // Default culture formatting, for pricing.etc
        var culture = new CultureInfo("da-DK");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        await builder.Build().RunAsync();
    }
}