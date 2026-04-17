using System.Net;
using System.Net.Http.Json;
using System.Threading;
namespace SwoopMarketplaceProjectFrontend.Services
{
    public class AuthApi
    {
        private readonly IHttpClientFactory _f;
        public AuthApi(IHttpClientFactory f) => _f = f;

        public async Task RegisterAsync(string email, string password, string phone, CancellationToken ct = default)
        {
            var client = _f.CreateClient("SwoopApi");
            var resp = await client.PostAsJsonAsync("api/auth/register", new { email, password, phone }, ct);

            if (resp.IsSuccessStatusCode)
                return;

            var body = await SafeReadBodyAsync(resp);
            throw new HttpRequestException(MapRegisterError(resp.StatusCode, body));
        }

        public async Task<string> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            var client = _f.CreateClient("SwoopApi");
            var resp = await client.PostAsJsonAsync("api/auth/login", new { email, password }, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await SafeReadBodyAsync(resp);
                throw new HttpRequestException(MapLoginError(resp.StatusCode, body));
            }

            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            return data?.Token ?? throw new InvalidOperationException("Nincs token a válaszban.");
        }

        private static async Task<string?> SafeReadBodyAsync(HttpResponseMessage resp)
        {
            try
            {
                return await resp.Content.ReadAsStringAsync();
            }
            catch { return null; }
        }

        private static string MapLoginError(HttpStatusCode code, string? body)
        {
            return code switch
            {
                HttpStatusCode.BadRequest => "Hibás kérés. Kérjük, ellenőrizze a mezőket.",
                HttpStatusCode.Unauthorized => "Helytelen e-mail cím vagy jelszó.",
                HttpStatusCode.Forbidden => "Nincs jogosultsága bejelentkezni.",
                HttpStatusCode.InternalServerError => "Szerverhiba. Próbálja újra később.",
                _ => !string.IsNullOrWhiteSpace(body) ? body! : $"Hiba történt: {(int)code} {code}"
            };
        }

        private static string MapRegisterError(HttpStatusCode code, string? body)
        {
            return code switch
            {
                HttpStatusCode.BadRequest => "Hibás adatok. Kérjük, töltse ki helyesen a mezőket.",
                HttpStatusCode.Conflict => "Ez az e-mail cím már regisztrálva van.",
                HttpStatusCode.InternalServerError => "Szerverhiba. Próbálja meg később.",
                _ => !string.IsNullOrWhiteSpace(body) ? body! : $"Hiba történt: {(int)code} {code}"
            };
        }

        private sealed class LoginResponse
        {
            public string Token { get; set; } = "";
        }
    }
}
