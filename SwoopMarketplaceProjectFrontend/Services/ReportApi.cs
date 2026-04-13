using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class ReportApi
    {
        private readonly IHttpClientFactory _f;

        public ReportApi(IHttpClientFactory f) => _f = f;

        public async Task<List<ReportDto>> GetAllAsync()
            => await _f.CreateClient("SwoopApi").GetFromJsonAsync<List<ReportDto>>("api/Reports") ?? new();

        public async Task<ReportDto?> GetByAzonAsync(long azon)
            => await _f.CreateClient("SwoopApi").GetFromJsonAsync<ReportDto>($"api/Reports/{azon}");

        public async Task CreateAsync(ReportDto dto)
        {
            var r = await _f.CreateClient("SwoopApi").PostAsJsonAsync("api/Reports", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(long azon, ReportDto dto)
        {
            var r = await _f.CreateClient("SwoopApi").PutAsJsonAsync($"api/Reports/{azon}", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(long azon)
        {
            var r = await _f.CreateClient("SwoopApi").DeleteAsync($"api/Reports/{azon}");
            r.EnsureSuccessStatusCode();
        }
    }
}
