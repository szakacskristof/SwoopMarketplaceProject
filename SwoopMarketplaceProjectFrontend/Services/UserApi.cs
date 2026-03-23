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

        // New helper: find user by email via the Users endpoint
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var client = _f.CreateClient("SwoopApi");
            try
            {
                var all = await client.GetFromJsonAsync<List<UserDto>>("api/Users") ?? new();
                return all.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }


        public async Task UpdateAsync(long azon, UserDto dto)
        {
            // map to the small update payload expected by the backend
            var payload = new
            {
                Id=dto.Id,
                Username = dto.Username,
                Email=dto.Email,
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
