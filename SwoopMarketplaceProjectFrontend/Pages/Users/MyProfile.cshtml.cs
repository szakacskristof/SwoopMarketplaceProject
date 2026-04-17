using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProject.Models;
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

        // file upload bind
        [BindProperty]
        public IFormFile? ProfileImageFile { get; set; }

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
            var u = users.FirstOrDefault(x => x.Email == email);
            if (u is null)
                return NotFound();

            User = u;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User is null) return BadRequest();

            if (!_auth.IsSignedIn) return RedirectToPage("/Account/Login");

            try
            {
                // If a new profile image was selected, upload it first
                if (ProfileImageFile != null && ProfileImageFile.Length > 0)
                {
                    var imageUrl = await _userApi.UploadProfileImageAsync(User.Id, ProfileImageFile);
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        User.ProfileImageUrl = imageUrl;
                    }
                }

                await _userApi.UpdateAsync(User.Id, User);

                Message = "A profil mentése sikerült.";
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
