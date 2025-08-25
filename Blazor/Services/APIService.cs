namespace Blazor.Services
{
    public partial class APIService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
    }
}