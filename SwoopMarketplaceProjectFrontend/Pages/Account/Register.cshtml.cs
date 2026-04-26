using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;
using System.ComponentModel.DataAnnotations;

namespace SwoopMarketplaceProjectFrontend.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AuthApi _auth;
        // Constructor: initialize register page with auth api.
        public RegisterModel(AuthApi auth) => _auth = auth;

        [BindProperty, Required(ErrorMessage = "Kötelezõ mezõ."), EmailAddress(ErrorMessage = "Kérjük, érvényes e-mail címet adjon meg.")]
        public string Email { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Kötelezõ mezõ.")]
        public string Phone { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Kötelezõ mezõ.")]
        public string Password { get; set; } = "";
        public string? Error { get; set; }
        // OnGet: show registration form.
        public void OnGet() { }

        // OnPostAsync: submit registration and redirect to login on success.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                await _auth.RegisterAsync(Email, Password, Phone);
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
