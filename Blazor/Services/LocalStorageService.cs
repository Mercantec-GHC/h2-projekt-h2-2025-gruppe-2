using Microsoft.JSInterop;

namespace Blazor.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveItemToStorageAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveToken", name, value);
    }

    public async Task<string> GetItemFromStorageAsync(string name)
    {
        string token = await _jsRuntime.InvokeAsync<string>("loginHelpers.loadItem", name);
        return token;
    }

    public async Task RemoveItemFromStorageAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.deleteItem", name);
    }
}