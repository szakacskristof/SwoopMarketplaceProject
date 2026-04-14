using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class AdminApi
    {
        private readonly IHttpClientFactory _f;
        public AdminApi(IHttpClientFactory f) => _f = f;

        public async Task SetUserRoleAsync(long userId, string role)
        {
            var client = _f.CreateClient("SwoopApi");
            // send property name matching server DTO
            var r = await client.PostAsJsonAsync($"api/Admin/users/{userId}/set-role", new { Role = role });
            if (!r.IsSuccessStatusCode)
            {
                var body = await r.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Set role failed: {(int)r.StatusCode} {r.ReasonPhrase} - {body}");
            }
        }
    }
}
