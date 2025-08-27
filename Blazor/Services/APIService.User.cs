using System.Net.Http.Json;
using System.Text.Json;
using DomainModels;

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

    public async Task<(bool status, string msg, string token)> LoginUser(string email, string password)
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
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return (true, "Login successfull!", loginResponse.token);
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Test: " + errorMsg);
                return (false,
                    "Failed to create new user! Code: " + response.StatusCode + ", " + errorMsg, "");
            }
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("Failed to parse JSON response, JSON is null: " + ex.Message);
            return (false, "Failed to parse JSON response, JSON is null: " + ex.Message, "");
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Failed to parse JSON response: " + ex.Message);
            return (false, "Failed to parse JSON response: " + ex.Message, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Generel exception caught, please contact support:" + ex.Message);
            return (false, "Generel exception caught, please contact support:  " + ex.Message, "");
        }
    }

    public async Task<(bool status, string msg)> ValidateToken(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Users/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<UserLoginDto>();
                Console.WriteLine("Token: " + tokenResponse.email);
                Console.WriteLine("Token: " + tokenResponse.id);
                Console.WriteLine("Token: " + tokenResponse.role);
                Console.WriteLine("Token: " + tokenResponse.username);
                return (true, "Token is valid");
            }

            return (false, "Token is invalid");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, "Generel exception caught, validating token: " + ex.Message);
        }
    }
}