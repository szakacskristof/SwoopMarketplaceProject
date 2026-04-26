using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;
using System.ComponentModel.DataAnnotations;

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

        // Constructor: initialize login page with auth api and session helper.

        [BindProperty, Required(ErrorMessage = "Kötelezõ mezõ."), EmailAddress(ErrorMessage = "Kérjük, érvényes e-mail címet adjon meg.")]
        public string Email { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Kötelezõ mezõ.")]
        public string Password { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }
        // OnGet: display login form (no pre-loading required).
        public void OnGet() { }

        // OnPostAsync: authenticate using AuthApi, store JWT and sign-in cookie.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var token = await _authApi.LoginAsync(Email, Password);
                _session.SetToken(token);

                // create authentication cookie principal from JWT so [Authorize] works
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();

                // Ensure authentication scheme is cookie-based (matches Program.cs)
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (!string.IsNullOrWhiteSpace(ReturnUrl))
                    return Redirect(ReturnUrl);
                return RedirectToPage("/Listings/Index");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }
    }
}
