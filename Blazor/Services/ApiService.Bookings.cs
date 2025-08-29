using System.Net.Http.Json;
using DomainModels;

namespace Blazor.Services;

public partial class APIService
{
        public async Task<List<Room>> GetRoomsByUserId(string userId, CancellationToken ct = default)
    {
        // API returns { message, rooms } — we only need rooms for the UI
        var res = await _httpClient.GetFromJsonAsync<UserRoomsResponse>($"api/Rooms/userId?userId={Uri.EscapeDataString(userId)}", ct);
        return res?.rooms ?? new();
    }

    private sealed class UserRoomsResponse
    {
        public string? message { get; set; }
        public List<Room>? rooms { get; set; }
    }   
}
