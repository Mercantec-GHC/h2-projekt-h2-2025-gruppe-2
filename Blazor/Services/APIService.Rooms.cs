using System.Net.Http.Json;
using System.Text.Json;
using DomainModels;

namespace Blazor.Services;

public partial class APIService
{
    public async Task<List<Room>> GetRooms(CancellationToken ct = default)
    {
        var res = await _httpClient.GetFromJsonAsync<List<Room>>("api/Rooms", ct);
        return res ?? new();
    }
}

public partial class APIService
{
    public async Task<Room> GetRoomById(int id, CancellationToken ct = default)
    {
        var res = await _httpClient.GetFromJsonAsync<Room>($"api/Rooms/{id}");
        return res ?? new();
    }
}