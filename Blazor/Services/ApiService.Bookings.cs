using System.Net.Http.Json;
using System.Text.Json;
using DomainModels;

namespace Blazor.Services;

public partial class APIService
{
    public async Task<List<Room>> GetRoomsByUserId(string userId, CancellationToken ct = default)
    {
        // API returns { message, rooms } — we only need rooms for the UI
        var res = await _httpClient.GetFromJsonAsync<UserRoomsResponse>(
            $"api/Rooms/userId?userId={Uri.EscapeDataString(userId)}", ct);
        return res?.rooms ?? new();
    }

    public async Task<string> CreateBooking(BookingDto bookingDto, string jwtToken, CancellationToken ct = default)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/Bookings")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(bookingDto, options),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };


            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                return "New booking created";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return errorContent;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "Generel exception caught, creating booking: " + ex.Message;
        }

        return "Booking failed";
    }


    private sealed class UserRoomsResponse
    {
        public string? message { get; set; }
        public List<Room>? rooms { get; set; }
    }
}