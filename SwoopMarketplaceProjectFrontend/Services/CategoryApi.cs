using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class CategoryApi
    {
        private readonly IHttpClientFactory _f;

        public CategoryApi(IHttpClientFactory f) => _f = f;


        public async Task<List<CategoryDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<CategoryDto>>("api/Categories") ?? new();


        public async Task<CategoryDto?> GetByAzonAsync(int azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<CategoryDto>($"api/Categories/{azon}");


        public async Task CreateAsync(CategoryDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Categories", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task UpdateAsync(int azon, CategoryDto dto)

        {

            var r = await _f.CreateClient("SwoopApi")

            .PutAsJsonAsync($"api/Categories/{azon}", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task DeleteAsync(int azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/Categories/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
