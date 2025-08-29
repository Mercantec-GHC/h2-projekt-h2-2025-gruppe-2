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

    // GET: api/Rooms/userId?userId={id}
    public async Task<List<Room>> GetUserRoomsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var url = $"api/Rooms/userId?userId={Uri.EscapeDataString(userId)}";
            var res = await _httpClient.GetFromJsonAsync<RoomsByUserResponse>(url, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, ct);
            return res?.rooms ?? new List<Room>();
        }
        catch
        {
            return new List<Room>();
        }
    }

    private sealed class RoomsByUserResponse
    {
        public string? message { get; set; }
        public List<Room>? rooms { get; set; }
    }
}