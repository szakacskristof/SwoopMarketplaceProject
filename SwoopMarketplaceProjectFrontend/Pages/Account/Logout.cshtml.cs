using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly AuthSession _auth;
        public LogoutModel(AuthSession auth) => _auth = auth;
        public IActionResult OnPost()
        {
            _auth.Clear();
            return RedirectToPage("/Index");
        }
    }
}
