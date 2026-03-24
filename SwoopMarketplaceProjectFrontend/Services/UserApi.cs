using SwoopMarketplaceProject.Models;
using SwoopMarketplaceProjectFrontend.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        // helper to lookup by email (frontend uses this)
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task CreateAsync(UserDto dto)
        {
            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Users", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(long azon, UserDto dto)
        {
            // The backend accepts optional Id and Email in the update DTO.
            var payload = new
            {
                Id = dto.Id,
                Email = dto.Email,
                Username = dto.Username,
                Phone = dto.Phone,
                ProfileImageUrl = dto.ProfileImageUrl,
                Bio = dto.Bio
            };

            var client = _f.CreateClient("SwoopApi");
            var r = await client.PutAsJsonAsync($"api/Users/{azon}", payload);

            if (!r.IsSuccessStatusCode)
            {
                var body = await r.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Update failed: {(int)r.StatusCode} {r.ReasonPhrase} - {body}");
            }
        }

        // Upload profile image for user id, returns the stored image url
        public async Task<string?> UploadProfileImageAsync(long userId, IFormFile file)
        {
            using var client = _f.CreateClient("SwoopApi");
            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync($"api/Users/{userId}/upload-photo", content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Upload failed: {(int)response.StatusCode} {response.ReasonPhrase} - {body}");
            }

            var data = await response.Content.ReadFromJsonAsync<UploadResponse?>();
            return data?.ImageUrl;
        }

        private class UploadResponse { public string ImageUrl { get; set; } = ""; }

        public async Task DeleteAsync(long azon)
        {
            var r = await _f.CreateClient("SwoopApi")
            .DeleteAsync($"api/Users/{azon}");

            r.EnsureSuccessStatusCode();
        }
    }
}
