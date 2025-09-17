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

    public async Task<Room> GetRoomById(string id, CancellationToken ct = default)
    {
        var res = await _httpClient.GetFromJsonAsync<Room>($"api/Rooms/{id}");
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

    public async Task<List<Room>> GetRoomsByAvailableDates(DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        try
        {
            var response =
                await _httpClient.GetAsync(
                    $"api/Rooms/availability?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Room>>();
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught getting rooms by availability: " + ex.Message);
            return [];
        }

        return [];
    }

    public async Task<(string msg, RoomOccupation? roomOccupation)> GetRoomOccupationsAsync(string roomId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Rooms/occupied/{roomId}", ct);

            if (response.IsSuccessStatusCode)
            {
                return ("OK", await response.Content.ReadFromJsonAsync<RoomOccupation>(cancellationToken: ct));
            }

            return (
                "Server error getting room occupation: " + response.ReasonPhrase +
                response.Content.ReadAsStringAsync(ct).Result, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ("Error getting room occupation: " + ex.Message, null);
        }
    }

    private sealed class RoomsByUserResponse
    {
        public string? message { get; set; }
        public List<Room>? rooms { get; set; }
    }
}