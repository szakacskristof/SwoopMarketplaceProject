using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class ListingApi
    {
        private readonly IHttpClientFactory _f;
        public ListingApi(IHttpClientFactory f) => _f = f;

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
            // fetch owner emails for distinct user ids
            var userIds = listings.Select(l => l.UserId).Distinct();
            var ownerMap = new Dictionary<long, string?>();
            foreach (var uid in userIds)
            {
                try
                {
                    var u = await client.GetFromJsonAsync<UserDto>($"api/Users/{uid}");
                    ownerMap[uid] = u?.Email;
                }
                catch
                {
                    ownerMap[uid] = null;
                }
            }
            foreach (var l in listings)
            {
                ownerMap.TryGetValue(l.UserId, out var email);
                result.Add(new ListingWithOwnerDto
                {
                    Listing = l,
                    OwnerEmail = email
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
            try
            {
                var u = await client.GetFromJsonAsync<UserDto>($"api/Users/{listing.UserId}");
                ownerEmail = u?.Email;
            }
            catch { }
            return new ListingWithOwnerDto { Listing = listing, OwnerEmail = ownerEmail };
        }

        public async Task CreateAsync(ListingDto dto)
        {
            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Listings", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(int azon, ListingDto dto)
        {
            var r = await _f.CreateClient("SwoopApi")
            .PutAsJsonAsync($"api/Listings/{azon}", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int azon)
        {
            var r = await _f.CreateClient("SwoopApi")
            .DeleteAsync($"api/Listings/{azon}");
            r.EnsureSuccessStatusCode();
        }
    }
}
