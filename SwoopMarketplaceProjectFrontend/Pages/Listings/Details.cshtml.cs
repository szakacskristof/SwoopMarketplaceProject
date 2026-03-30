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
        public DetailsModel(ListingApi api, UserApi userApi, AuthSession auth)
        {
            _api = api;
            _userApi = userApi;
            _auth = auth;
        }
        public ListingDto? Listing { get; private set; }
        public string? OwnerEmail { get; private set; }

        // New: optional owner profile image URL
        public string? OwnerProfileImageUrl { get; private set; }

        // show/hide controls separately so behavior matches Index:
        // - Edit: only owner
        // - Delete: owner OR Admin
        public bool CanEdit { get; private set; }
        public bool CanDelete { get; private set; }

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

            var currentEmail = _auth.GetEmail();
            var isAdmin = _auth.IsInRole("Admin");

            // owner may edit; admin should not be allowed to edit (keep consistent with Index)
            CanEdit = !string.IsNullOrWhiteSpace(currentEmail) && string.Equals(currentEmail, OwnerEmail, StringComparison.OrdinalIgnoreCase);

            // delete allowed for owner or admin
            CanDelete = isAdmin || CanEdit;

            return Page();
        }
    }
}
