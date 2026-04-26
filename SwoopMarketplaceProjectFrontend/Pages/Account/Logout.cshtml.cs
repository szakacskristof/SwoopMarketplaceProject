using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly AuthSession _auth;
        // Constructor: initialize logout page with auth session.
        public LogoutModel(AuthSession auth) => _auth = auth;

        // OnPost: clear session token and redirect to home.
        public IActionResult OnPost()
        {
            _auth.Clear();
            return RedirectToPage("/Index");
        }
    }
}
