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
        private readonly ListingApi _listingApi;

        public MyProfileModel(UserApi userApi, AuthSession auth, ListingApi listingApi)
        {
            _userApi = userApi;
            _auth = auth;
            _listingApi = listingApi;
        }

        [BindProperty]
        public UserDto? User { get; set; }

        // file upload bind
        [BindProperty]
        public IFormFile? ProfileImageFile { get; set; }

        public string? Message { get; set; }
        public string? Error { get; set; }

        // User's own listings (display on the page)
        public List<ListingWithOwnerDto>? UserListings { get; set; }

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

            // Load all listings with owner info, then filter for this user
            try
            {
                var all = await _listingApi.GetAllWithOwnersAsync();
                UserListings = all.Where(l => l.Listing.UserId == User.Id).ToList();
            }
            catch
            {
                // ignore listing load errors; keep profile usable
                UserListings = new List<ListingWithOwnerDto>();
            }

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
