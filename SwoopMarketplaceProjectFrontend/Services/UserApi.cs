using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class UserApi
    {
        private readonly IHttpClientFactory _f;

        public UserApi(IHttpClientFactory f) => _f = f;


        public async Task<List<CategoryDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<CategoryDto>>("api/Users") ?? new();


        public async Task<UserDto?> GetByAzonAsync(int azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<UserDto>($"api/Users/{azon}");


        public async Task CreateAsync(UserDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Users", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task UpdateAsync(int azon, UserDto dto)

        {

            var r = await _f.CreateClient("SwoopApi")

            .PutAsJsonAsync($"api/Users/{azon}", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task DeleteAsync(int azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/Users/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
