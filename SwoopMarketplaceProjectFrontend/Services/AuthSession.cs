using System.IdentityModel.Tokens.Jwt;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class AuthSession
    {
        private const string TokenKey = "auth.jwt";
        private readonly IHttpContextAccessor _http;

        public AuthSession(IHttpContextAccessor http) => _http = http;

        public string? GetToken()
        => _http.HttpContext?.Session.GetString(TokenKey);

        public void SetToken(string token)
        => _http.HttpContext?.Session.SetString(TokenKey, token);

        public void Clear()
        => _http.HttpContext?.Session.Remove(TokenKey);

        public bool IsSignedIn
        => !string.IsNullOrWhiteSpace(GetToken());
        public string? GetEmail()
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return null;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        }
    }
}
