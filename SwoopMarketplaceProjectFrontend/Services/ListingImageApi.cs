using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class ListingImageApi
    {
        private readonly IHttpClientFactory _f;

        public ListingImageApi(IHttpClientFactory f) => _f = f;


        public async Task<List<ListingImageDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<ListingImageDto>>("api/ListingImages") ?? new();


        public async Task<ListingImageDto?> GetByAzonAsync(int azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<ListingImageDto>($"api/ListingImages/{azon}");


        public async Task CreateAsync(ListingImageDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/ListingImages", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task UpdateAsync(int azon, ListingImageDto dto)

        {

            var r = await _f.CreateClient("SwoopApi")

            .PutAsJsonAsync($"api/ListingImages/{azon}", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task DeleteAsync(int azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/ListingImages/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
