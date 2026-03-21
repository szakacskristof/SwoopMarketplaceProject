using SwoopMarketplaceProject.Models;
using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class UserApi
    {
        private readonly IHttpClientFactory _f;

        public UserApi(IHttpClientFactory f) => _f = f;


        public async Task<List<UserDto>> GetAllAsync()

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<List<UserDto>>("api/Users") ?? new();


        public async Task<UserDto?> GetByAzonAsync(long azon)

        => await _f.CreateClient("SwoopApi")

        .GetFromJsonAsync<UserDto>($"api/Users/{azon}");


        public async Task CreateAsync(UserDto dto)

        {

            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Users", dto);

            r.EnsureSuccessStatusCode();

        }


        public async Task UpdateAsync(long azon, UserDto dto)
        {
            // map to the small update payload expected by the backend
            var payload = new
            {
                Username = dto.Username,
                Phone = dto.Phone,
                ProfileImageUrl = dto.ProfileImageUrl,
                Bio = dto.Bio
            };

            var r = await _f.CreateClient("SwoopApi")
                .PutAsJsonAsync($"api/Users/{azon}", payload);

            r.EnsureSuccessStatusCode();
        }

    
        


        public async Task DeleteAsync(long azon)

        {

            var r = await _f.CreateClient("SwoopApi")

            .DeleteAsync($"api/Users/{azon}");

            r.EnsureSuccessStatusCode();

        }
    }
}
