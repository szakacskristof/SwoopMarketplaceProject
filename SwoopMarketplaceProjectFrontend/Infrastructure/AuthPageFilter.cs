using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SwoopMarketplaceProjectFrontend.Services;
namespace SwoopMarketplaceProjectFrontend.Infrastructure
{
    public class AuthPageFilter : IAsyncPageFilter
    {
        private readonly AuthSession _auth;
        public AuthPageFilter(AuthSession auth) => _auth = auth;
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        => Task.CompletedTask;

        public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
        {
            var path = context.HttpContext.Request.Path.Value ?? "/";
            // Bizonyos oldalakat megvédünk
            if (
            path.StartsWith("/Listings", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Users", StringComparison.OrdinalIgnoreCase))
            {
                if (!_auth.IsSignedIn)
                {
                    context.Result = new RedirectToPageResult("/Account/Login",
                    new { returnUrl = path + context.HttpContext.Request.QueryString });
                    return Task.CompletedTask;
                }
            }
            return next();
        }
    }
}
