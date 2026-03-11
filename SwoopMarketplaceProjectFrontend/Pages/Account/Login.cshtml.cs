using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AuthApi _authApi;
        private readonly AuthSession _session;

        public LoginModel(AuthApi authApi, AuthSession session)
        {
            _authApi = authApi;
            _session = session;

        }

        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }
        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var token = await _authApi.LoginAsync(Email, Password);
                _session.SetToken(token);
                if (!string.IsNullOrWhiteSpace(ReturnUrl))
                    return Redirect(ReturnUrl);
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }
    }
}
