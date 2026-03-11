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
            if (MatchesAnyPrefix(path, SignedInPrefixes))
            {
                if (!_auth.IsSignedIn)
                {
                    context.Result = new RedirectToPageResult("/Account/Login",
                    new { returnUrl = path + context.HttpContext.Request.QueryString });
                    return Task.CompletedTask;
                }
                if (MatchesAnyPrefix(path, AdminOnlyPrefixes) && !_auth.IsInRole("Admin"))
                {
                    context.Result = new RedirectToPageResult("/Errors/Forbidden");
                    return Task.CompletedTask;
                }
            }
            return next();
        }


        private static bool MatchesAnyPrefix(string path, string[] prefixes)

        => prefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        // Ide soroljuk a védett útvonalakat prefix szerint
        private static readonly string[] SignedInPrefixes =
        [
            "/Listings",
            "/Categories"
        ];

        // Admin oldalak: lehet prefix vagy konkrét útvonal
        private static readonly string[] AdminOnlyPrefixes =
        [  
            "/Users/Delete",
            "/Users/Details"

        ];





    }
}
