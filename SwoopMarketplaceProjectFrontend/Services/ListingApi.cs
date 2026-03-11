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
