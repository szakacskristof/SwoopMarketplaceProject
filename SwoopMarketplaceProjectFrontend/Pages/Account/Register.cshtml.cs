using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AuthApi _auth;
        public RegisterModel(AuthApi auth) => _auth = auth;
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        public string? Error { get; set; }
        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _auth.RegisterAsync(Email, Password);
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }
    }
}
