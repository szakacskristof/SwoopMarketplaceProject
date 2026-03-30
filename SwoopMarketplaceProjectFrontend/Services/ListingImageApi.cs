using SwoopMarketplaceProjectFrontend.Dtos;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        // Upload file and return created ListingImageDto (reads CreatedAtAction payload)
        public async Task<ListingImageDto> UploadAsync(long listingId, IFormFile file)
        {
            using var client = _f.CreateClient("SwoopApi");

            using var content = new MultipartFormDataContent();

            // file content
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.FileName);

            // listing id field
            content.Add(new StringContent(listingId.ToString()), "listingId");

            var r = await client.PostAsync("api/ListingImages/upload", content);
            r.EnsureSuccessStatusCode();

            // try deserialize the created object
            var created = await r.Content.ReadFromJsonAsync<ListingImageDto>();
            return created ?? new ListingImageDto { Id = 0, ListingId = listingId, ImageUrl = "" };
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

        // Set primary image for a listing (server provides atomic endpoint)
        public async Task SetPrimaryAsync(long listingId, long primaryImageId)
        {
            var r = await _f.CreateClient("SwoopApi").PostAsync($"api/ListingImages/{primaryImageId}/set-primary", null);
            r.EnsureSuccessStatusCode();
        }
    }
}
