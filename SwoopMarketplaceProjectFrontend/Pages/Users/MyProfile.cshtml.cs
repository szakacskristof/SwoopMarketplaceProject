using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Users
{
    public class MyProfileModel : PageModel
    {
        private readonly UserApi _userApi;
        private readonly AuthSession _auth;

        public MyProfileModel(UserApi userApi, AuthSession auth)
        {
            _userApi = userApi;
            _auth = auth;
        }

        [BindProperty]
        public UserDto? User { get; set; }

        public string? Message { get; set; }
        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            var email = _auth.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Forbid();

            var users = await _userApi.GetAllAsync();
            var u = users.FirstOrDefault(x =>x.Email==email);
            if (u is null)
                return NotFound();

            User = u;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User is null) return BadRequest();

            if (!_auth.IsSignedIn) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _userApi.UpdateAsync(User.Id, User);

                Message = "Profile saved.";
                return RedirectToPage(); // refresh and re-load
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }
    }
}
