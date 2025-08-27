using Microsoft.JSInterop;

namespace Blazor.Services;

public class StorageService
{
    private readonly IJSRuntime _jsRuntime;

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveItemToStorageAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveItemToLocal", name, value);
    }
    
    public async Task SaveItemToSessionAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveItemToSession", name, value);
    }

    public async Task<string> GetItemFromStorageAsync(string name)
    {
        string token = await _jsRuntime.InvokeAsync<string>("loginHelpers.getItemFromLocal", name);
        return token;
    }
    
    public async Task<string> GetItemFromSessionStorageAsync(string name)
    {
        string token = await _jsRuntime.InvokeAsync<string>("loginHelpers.getItemFromSession", name);
        return token;
    }

    public async Task RemoveItemFromStorageAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.deleteItem", name);
    }
}