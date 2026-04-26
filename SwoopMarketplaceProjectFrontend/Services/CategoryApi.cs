using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class CategoryApi
    {
        private readonly IHttpClientFactory _f;

        // Constructor: initialize with HTTP client factory.
        public CategoryApi(IHttpClientFactory f) => _f = f;


        // GetAllAsync: retrieve all categories from API.
        public async Task<List<CategoryDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<CategoryDto>>("api/Categories") ?? new();


        // GetByAzonAsync: retrieve a category by id.
        public async Task<CategoryDto?> GetByAzonAsync(int azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<CategoryDto>($"api/Categories/{azon}");


        // CreateAsync: create a new category on the server.
        public async Task CreateAsync(CategoryDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Categories", dto);

            r.EnsureSuccessStatusCode();

        }


        // UpdateAsync: update a category by id.
        public async Task UpdateAsync(int azon, CategoryDto dto)

        {

            var r = await _f.CreateClient("SwoopApi")

            .PutAsJsonAsync($"api/Categories/{azon}", dto);

            r.EnsureSuccessStatusCode();

        }


        // DeleteAsync: delete a category by id.
        public async Task DeleteAsync(int azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/Categories/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
