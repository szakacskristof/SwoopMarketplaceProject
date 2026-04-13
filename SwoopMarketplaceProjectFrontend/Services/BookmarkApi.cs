using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class BookmarkApi
    {
        private readonly IHttpClientFactory _f;
        private readonly AuthSession _auth;

        public BookmarkApi(IHttpClientFactory f, AuthSession auth) { _f = f; _auth = auth; }

        public async Task<List<long>> GetForCurrentUserAsync()
        {
            var client = _f.CreateClient("SwoopApi");

            using var req = new HttpRequestMessage(HttpMethod.Get, "api/Bookmarks");
            var token = _auth.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.SendAsync(req);
            // If user is not authenticated or forbidden, return empty list so UI can still render.
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return new List<long>();
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {body}");
            }

            var ids = await resp.Content.ReadFromJsonAsync<List<long>>();
            return ids ?? new List<long>();
        }

        public async Task AddAsync(long listingId)
        {
            var client = _f.CreateClient("SwoopApi");
            using var req = new HttpRequestMessage(HttpMethod.Post, "api/Bookmarks")
            {
                Content = JsonContent.Create(new { listingId })
            };
            var token = _auth.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var r = await client.SendAsync(req);
            r.EnsureSuccessStatusCode();
        }

        public async Task RemoveAsync(long listingId)
        {
            var client = _f.CreateClient("SwoopApi");
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Bookmarks/{listingId}");
            var token = _auth.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var r = await client.SendAsync(request);
            // allow 204 or 200
            if (!r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.NoContent)
                r.EnsureSuccessStatusCode();
        }
    }
}