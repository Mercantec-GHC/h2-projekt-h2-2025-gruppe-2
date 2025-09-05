using System.Text.Json;
using DomainModels;
using Microsoft.JSInterop;

namespace Blazor.Services;

public class StorageService
{
    private readonly IJSRuntime _jsRuntime;

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveItemToLocalStorageAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveItemToLocal", name, value);
    }
    
    private async Task SaveItemToSessionStorageAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveItemToSession", name, value);
    }

    public async Task<string?> GetItemFromLocalStorageAsync(string name)
    {
        string? token = await _jsRuntime.InvokeAsync<string?>("loginHelpers.getItemFromLocal", name);
        return token;
    }
    
    public async Task<string> GetItemFromSessionStorageAsync(string name)
    {
        string token = await _jsRuntime.InvokeAsync<string>("loginHelpers.getItemFromSession", name);
        return token;
    }

    public async Task RemoveItemFromLocalStorageAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.deleteItemFromLocal", name);
    }
    public async Task RemoveItemFromSessionStorageAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.deleteItemFromSession", name);
    }
    
    public async Task MakeSessionToken(SessionTokenDto sessionTokenDto)
    {
        var sessionTokenJson = System.Text.Json.JsonSerializer.Serialize(sessionTokenDto);
        await SaveItemToSessionStorageAsync("user", sessionTokenJson);
    }

    public async Task SaveCacheObjectToLocalStorageAsync<T>(string cacheName, T cacheData , TimeSpan cacheDuration)
    {
        var cache = new CacheWrapper<T>
        {
            Data = cacheData,
            Expiry = DateTime.UtcNow.Add(cacheDuration)
        };
        var json = JsonSerializer.Serialize(cache);
        await SaveItemToLocalStorageAsync(cacheName, json);
    }

    public async Task<CacheWrapper<T>?> GetCacheObjectFromLocalStorageAsync<T>(string cacheName)
    {
        string? json = await GetItemFromLocalStorageAsync(cacheName);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<CacheWrapper<T>>(json);
    }
}