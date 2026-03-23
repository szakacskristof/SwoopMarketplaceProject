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

        // Look for several common email claim names used by different identity providers
        public string? GetEmail()
        {
            var jwt = ReadJwt();
            if (jwt is null) return null;

            // Check typical claim types: ClaimTypes.Email, "email", JwtRegisteredClaimNames.Email
            var claim = jwt.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, StringComparison.OrdinalIgnoreCase)
            );

            return claim?.Value;
        }

        // Try to extract numeric user id from common claim names: nameid, sub, id, userId
        // Returns null if not present or not parseable as long.
        public long? GetUserId()
        {
            var jwt = ReadJwt();
            if (jwt is null) return null;

            var claim = jwt.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "nameid", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "id", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "userId", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "uid", StringComparison.OrdinalIgnoreCase)
            );

            if (claim == null) return null;

            if (long.TryParse(claim.Value, out var id)) return id;

            // sometimes sub or other claim may be GUID or string; ignore those
            return null;
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
