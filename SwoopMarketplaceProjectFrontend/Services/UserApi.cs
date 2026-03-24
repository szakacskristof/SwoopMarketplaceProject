using SwoopMarketplaceProject.Models;
using SwoopMarketplaceProjectFrontend.Dtos;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class UserApi
    {
        private readonly IHttpClientFactory _f;

        public UserApi(IHttpClientFactory f) => _f = f;

        public async Task<List<UserDto>> GetAllAsync()
        {
            var client = _f.CreateClient("SwoopApi");
            var users = await client.GetFromJsonAsync<List<UserDto>>("api/Users") ?? new();

            // Normalize profile image URLs to absolute URLs so frontend img src points to the backend host
            if (client.BaseAddress is not null)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    var u = users[i];
                    if (!string.IsNullOrWhiteSpace(u.ProfileImageUrl))
                    {
                        var raw = u.ProfileImageUrl.Trim();
                        if (Uri.TryCreate(raw, UriKind.Absolute, out var _))
                        {
                            // already absolute - keep
                            users[i].ProfileImageUrl = raw;
                        }
                        else
                        {
                            // convert root-relative or relative to absolute using client's BaseAddress
                            users[i].ProfileImageUrl = new Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
                        }
                    }
                }
            }

            return users;
        }

        public async Task<UserDto?> GetByAzonAsync(long azon)
        {
            var client = _f.CreateClient("SwoopApi");
            var user = await client.GetFromJsonAsync<UserDto>($"api/Users/{azon}");
            if (user is not null && client.BaseAddress is not null && !string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                var raw = user.ProfileImageUrl.Trim();
                if (!Uri.TryCreate(raw, UriKind.Absolute, out _))
                    user.ProfileImageUrl = new Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
            }
            return user;
        }

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
            var imageUrl = data?.ImageUrl;

            // normalize returned imageUrl to absolute if necessary
            if (!string.IsNullOrWhiteSpace(imageUrl) && client.BaseAddress is not null)
            {
                var raw = imageUrl.Trim();
                if (!Uri.TryCreate(raw, UriKind.Absolute, out _))
                    imageUrl = new Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
            }

            return imageUrl;
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
