using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        // --- JWT claims kiolvasásának segédfüggvényei ---

        private JwtSecurityToken? ReadJwt()
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return null;
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        public string? GetEmail()
        {
            var jwt = ReadJwt();
            return jwt?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        }
        public IReadOnlyList<string> GetRoles()
        {
            var jwt = ReadJwt();
            if (jwt is null) return Array.Empty<string>();
            // A role claim tipikusan: "role" (ClaimTypes.Role is erre mapelődik JWT-ben)
            return jwt.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        }

        public bool IsInRole(string role)
        => GetRoles().Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
