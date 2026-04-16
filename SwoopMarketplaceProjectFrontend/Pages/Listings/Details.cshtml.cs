using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class DetailsModel : PageModel
    {
        private readonly ListingApi _api;
        private readonly UserApi _userApi;
        private readonly AuthSession _auth;
        private readonly BookmarkApi _bookmarkApi;

        public DetailsModel(ListingApi api, UserApi userApi, AuthSession auth, BookmarkApi bookmarkApi)
        {
            _api = api;
            _userApi = userApi;
            _auth = auth;
            _bookmarkApi = bookmarkApi;
        }
        public ListingDto? Listing { get; private set; }
        public string? OwnerEmail { get; private set; }
        public string? OwnerProfileImageUrl { get; private set; }
        public string? OwnerPhone { get; private set; }
        public string? OwnerUserName { get; private set; }
        public bool CanEdit { get; private set; }
        public bool CanDelete { get; private set; }

        // frontend-only: whether current user bookmarked this listing
        public bool IsBookmarked { get; private set; }

        public async Task<IActionResult> OnGetAsync(int azon)
        {
            if (azon <= 0)
               return NotFound();
            var lw = await _api.GetByAzonWithOwnerAsync(azon);
            if (lw is null || lw.Listing is null)
                return NotFound();
            Listing = lw.Listing;
            OwnerEmail = lw.OwnerEmail;
            OwnerProfileImageUrl = lw.OwnerProfileImageUrl;

            // Load owner's phone 
            try
            {
                var owner = await _userApi.GetByAzonAsync(Listing.UserId);
                OwnerPhone = owner?.Phone;
            }
            catch
            {
                OwnerPhone = null;
            }
            // Load owner's username
            try
            {
                var owner = await _userApi.GetByAzonAsync(Listing.UserId);
               OwnerUserName = owner?.Username;
            }
            catch
            {
                OwnerUserName = null;
            }

            var currentEmail = _auth.GetEmail();
            var isAdmin = _auth.IsInRole("Admin");

            CanEdit = !string.IsNullOrWhiteSpace(currentEmail) && string.Equals(currentEmail, OwnerEmail, StringComparison.OrdinalIgnoreCase);
            CanDelete = isAdmin || CanEdit;

            if (_auth.IsSignedIn)
            {
                try
                {
                    var ids = await _bookmarkApi.GetForCurrentUserAsync();
                    IsBookmarked = ids.Contains(Listing.Id);
                }
                catch { IsBookmarked = false; }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleBookmarkAsync(int azon)
        {
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Details", new { azon }) });
            }

            try
            {
                var ids = await _bookmarkApi.GetForCurrentUserAsync();
                if (ids.Contains(azon))
                    await _bookmarkApi.RemoveAsync(azon);
                else
                    await _bookmarkApi.AddAsync(azon);
            }
            catch { }

            return RedirectToPage(new { azon });
        }
    }
}
