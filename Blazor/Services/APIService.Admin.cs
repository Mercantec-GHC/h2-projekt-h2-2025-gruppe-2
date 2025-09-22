using System.Net.Http.Headers;
using System.Net.Http.Json;
using DomainModels;

namespace Blazor.Services;

public partial class APIService
{
    public async Task<List<Booking>> GetAllBookingsAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<Booking>>("api/Bookings", ct);
            return list ?? new List<Booking>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching bookings: " + ex.Message);
            return new List<Booking>();
        }
    }

    public async Task<BookingDetails?> GetBookingDetailsAsync(string jwtToken, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Bookings/details");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<BookingDetails>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching booking details: " + ex.Message);
            return null;
        }
    }

    public async Task<TotalRoomsDetails?> GetRoomsDetailsAsync(string jwtToken, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Rooms/details");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TotalRoomsDetails>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching room details: " + ex.Message);
            return null;
        }
    }

    public async Task<UserDetails?> GetUserDetailsAsync(string jwtToken, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Users/details");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDetails>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching user details: " + ex.Message);
            return null;
        }
    }

    public async Task<List<User>> GetUsersAsync(string jwtToken, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return new List<User>();
            var users = await response.Content.ReadFromJsonAsync<List<User>>(cancellationToken: ct);
            return users ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching users: " + ex.Message);
            return new List<User>();
        }
    }
}
