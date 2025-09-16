using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainModels;
using System.Net.Http.Headers;

namespace Blazor.Services;

public partial class APIService
{
    public async Task<(bool status, string msg)> RegisterUser(string email, string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", new
            {
                email = email,
                username = username,
                password = password
            });

            if (response.IsSuccessStatusCode)
            {
                return (true, "Successfully created new user!");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Test: " + errorMsg);
                return (false,
                    "Failed to create new user! Code: " + response.StatusCode + ", " + errorMsg);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return (false, "Generel exception caught, please contact support:  " + ex.Message);
        }
    }

    public async Task<(string msg, LoginResponseDto? responseDto)> LoginUser(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/login", new
            {
                email = email,
                password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                return ("Login successfull!", loginResponse);
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Test: " + errorMsg);
                return ("Failed to login user! Code: " + response.StatusCode + ", " + errorMsg, null);
            }
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("Failed to parse JSON response, JSON is null: " + ex.Message);
            return ("Failed to parse JSON response, JSON is null: " + ex.Message, null);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Failed to parse JSON response: " + ex.Message);
            return ("Failed to parse JSON response: " + ex.Message, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Generel exception caught, please contact support:" + ex.Message);
            return ("Generel exception caught, please contact support:  " + ex.Message, null);
        }
    }

    public async Task<(bool status, string msg, SessionTokenDto? userDto)> ValidateToken(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Users/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var sessionTokenDto = await response.Content.ReadFromJsonAsync<SessionTokenDto>();
                return (true, "Ok", sessionTokenDto);
            }

            return (false, "Token is invalid", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, "Generel exception caught, validating token: " + ex.Message, null);
        }
    }

    public async Task<(bool status, string msg)> ChangeOwnPassword(string currentPassword, string newPassword,
        string token)
    {
        try
        {
            Console.WriteLine("Changing password with: " + token);
            var request = new HttpRequestMessage(HttpMethod.Put, "api/Users/me/password");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    currentPassword,
                    newPassword
                }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Password got changed");
            }

            return (false, "Token is invalid");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, "Generel exception caught, validating token: " + ex.Message);
        }
    }

    public async Task<(bool status, string msg, UserBookingsResponse? bookingsRooms)> GetUsersBookingsRooms(
        string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Bookings/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var userBookings = await response.Content.ReadFromJsonAsync<UserBookingsResponse>();
                return (true, "Ok", userBookings);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, $"Generel exception caught, getting a users ({userId}) bookings: " + ex.Message, null);
        }

        return (false, "Not implemented yet", null);
    }

    public async Task<bool> IsUserAdmin(string jwtToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Users/admin");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error authorizing user: " + ex.Message);
            return false;
        }

        return false;
    }
}