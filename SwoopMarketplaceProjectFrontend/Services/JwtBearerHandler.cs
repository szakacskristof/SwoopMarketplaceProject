using SwoopMarketplaceProjectFrontend.Services;
using System.Net.Http.Headers;

namespace ThormaFrontend.Services
{
    public class JwtBearerHandler : DelegatingHandler
    {
        private readonly AuthSession _session;
        public JwtBearerHandler(AuthSession session) => _session = session;
        protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        {
            var token = _session.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
