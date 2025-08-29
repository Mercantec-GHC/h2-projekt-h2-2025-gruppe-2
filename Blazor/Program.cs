using System;
using System.Net.Http;
using System.Globalization;
using Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Reads API endpoint from environment variables or uses default
        var envApiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
        Console.WriteLine($"API ENV Endpoint: {envApiEndpoint}");
        var apiEndpoint = envApiEndpoint ?? "https://karambit-api.mercantec.tech/";
        Console.WriteLine($"API Endpoint: {apiEndpoint}");

        // Registers HttpClient to API service with a configurable endpoint
        builder.Services.AddHttpClient<APIService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
            Console.WriteLine($"APIService BaseAddress: {client.BaseAddress}");
        });
        builder.Services.AddScoped<StorageService>(sp =>
            new StorageService(sp.GetRequiredService<IJSRuntime>()));
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<StorageService>(sp =>
            new StorageService(sp.GetRequiredService<IJSRuntime>()));
    builder.Services.AddScoped<ClientJwtService>();

        // Default culture formatting, for pricing.etc
        var culture = new CultureInfo("da-DK");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        await builder.Build().RunAsync();
    }
}
