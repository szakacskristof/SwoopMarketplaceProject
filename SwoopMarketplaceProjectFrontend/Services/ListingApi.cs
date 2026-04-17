using System.Net.Http.Headers;
using System.Net.Http.Json;
using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class ListingApi
    {
        private readonly IHttpClientFactory _f;
        private readonly AuthSession _auth;

        public ListingApi(IHttpClientFactory f, AuthSession auth) { _f = f; _auth = auth; }

        public async Task<List<ListingDto>> GetAllAsync()
            => await _f.CreateClient("SwoopApi")
                .GetFromJsonAsync<List<ListingDto>>("api/Listings") ?? new();

        public async Task<ListingDto?> GetByAzonAsync(int azon)
            => await _f.CreateClient("SwoopApi")
                .GetFromJsonAsync<ListingDto>($"api/Listings/{azon}");

        public async Task<List<ListingWithOwnerDto>> GetAllWithOwnersAsync()
        {
            var client = _f.CreateClient("SwoopApi");
            var listings = await client.GetFromJsonAsync<List<ListingDto>>("api/Listings") ?? new();
            var result = new List<ListingWithOwnerDto>();
            var userIds = listings.Select(l => l.UserId).Distinct();
            var ownerMapEmail = new Dictionary<long, string?>();
            var ownerMapImage = new Dictionary<long, string?>();
            var ownerMapUserName = new Dictionary<long, string?>();

            foreach (var uid in userIds)
            {
                try
                {
                    var u = await client.GetFromJsonAsync<UserDto>($"api/Users/{uid}");
                    var email = u?.Email;
                    var username = u?.Username;
                    string? profileImage = null;
                    if (u != null && !string.IsNullOrWhiteSpace(u.ProfileImageUrl))
                    {
                        var raw = u.ProfileImageUrl.Trim();
                        if (!Uri.TryCreate(raw, UriKind.Absolute, out _))
                            profileImage = new Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
                        else
                            profileImage = raw;
                    }
                    ownerMapEmail[uid] = email;
                    ownerMapImage[uid] = profileImage;
                    ownerMapUserName[uid] = username;
                }
                catch
                {
                    ownerMapEmail[uid] = null;
                    ownerMapImage[uid] = null;
                    ownerMapUserName[uid] = null;
                }
            }

            foreach (var l in listings)
            {
                ownerMapEmail.TryGetValue(l.UserId, out var email);
                ownerMapImage.TryGetValue(l.UserId, out var image);
                ownerMapUserName.TryGetValue(l.UserId, out var username);
                result.Add(new ListingWithOwnerDto
                {
                    Listing = l,
                    OwnerEmail = email,
                    OwnerProfileImageUrl = image,
                    OwnerUserName = username
                });
            }
            return result;
        }

        public async Task<ListingWithOwnerDto?> GetByAzonWithOwnerAsync(int azon)
        {
            var client = _f.CreateClient("SwoopApi");
            var listing = await client.GetFromJsonAsync<ListingDto>($"api/Listings/{azon}");
            if (listing is null) return null;
            string? ownerEmail = null;
            string? ownerImage = null;
            string? ownerUserName = null;
            try
            {
                var u = await client.GetFromJsonAsync<UserDto>($"api/Users/{listing.UserId}");
                ownerEmail = u?.Email;
                ownerUserName = u?.Username;
                if (u != null && !string.IsNullOrWhiteSpace(u.ProfileImageUrl))
                {
                    var raw = u.ProfileImageUrl.Trim();
                    ownerImage = !Uri.TryCreate(raw, UriKind.Absolute, out _)
                        ? new Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString()
                        : raw;
                }
            }
            catch { }
            return new ListingWithOwnerDto
            {
                Listing = listing,
                OwnerEmail = ownerEmail,
                OwnerProfileImageUrl = ownerImage,
                OwnerUserName = ownerUserName
            };
        }

        public async Task<ListingDto?> CreateAsync(ListingDto dto)
        {
            var client = _f.CreateClient("SwoopApi");
            var payload = new
            {
                dto.CategoryId, dto.Title, dto.Description, dto.Price,
                dto.Condition, Status = dto.Status ?? "active", dto.Location
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, "api/Listings")
            {
                Content = JsonContent.Create(payload)
            };
            var token = _auth.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var r = await client.SendAsync(req);
            if (!r.IsSuccessStatusCode)
            {
                var body = await r.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {(int)r.StatusCode} {r.StatusCode}: {body}");
            }
            return await r.Content.ReadFromJsonAsync<ListingDto>();
        }

        public async Task<ListingDto?> CreateWithUserAsync(ListingDto dto, UserDto user)
        {
            var client = _f.CreateClient("SwoopApi");
            var payload = new
            {
                dto.Id, dto.UserId, dto.CategoryId, CategoryName = dto.CategoryName,
                dto.Title, dto.Description, dto.Price, dto.Condition, dto.Status,
                dto.Location, dto.CreatedAt, dto.UpdatedAt,
                User = new
                {
                    user.Id, user.Username, user.Email, user.Phone,
                    user.ProfileImageUrl, user.Bio, user.CreatedAt
                }
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, "api/Listings")
            {
                Content = JsonContent.Create(payload)
            };
            var token = _auth.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var r = await client.SendAsync(req);
            if (!r.IsSuccessStatusCode)
            {
                var body = await r.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {(int)r.StatusCode} {r.StatusCode}: {body}");
            }
            return await r.Content.ReadFromJsonAsync<ListingDto>();
        }

        public async Task UpdateAsync(int azon, ListingDto dto)
        {
            var r = await _f.CreateClient("SwoopApi").PutAsJsonAsync($"api/Listings/{azon}", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int azon)
        {
            var r = await _f.CreateClient("SwoopApi").DeleteAsync($"api/Listings/{azon}");
            r.EnsureSuccessStatusCode();
        }
    }
}
