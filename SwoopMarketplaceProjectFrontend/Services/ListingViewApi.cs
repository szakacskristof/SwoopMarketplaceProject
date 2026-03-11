using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class ListingViewApi
    {
        private readonly IHttpClientFactory _f;

        public ListingViewApi(IHttpClientFactory f) => _f = f;


        public async Task<List<ListingViewDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<ListingViewDto>>("api/ListingViews") ?? new();


        public async Task<ListingViewDto?> GetByAzonAsync(int azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<ListingViewDto>($"api/ListingViews/{azon}");


        public async Task CreateAsync(ListingViewDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/ListingViews", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task UpdateAsync(int azon, ListingViewDto dto)

        {

            var r = await _f.CreateClient("SwoopApi")

            .PutAsJsonAsync($"api/ListingViews/{azon}", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task DeleteAsync(int azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/ListingViews/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
