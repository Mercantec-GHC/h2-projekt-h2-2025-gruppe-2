using System.Net.Http.Json;

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
}