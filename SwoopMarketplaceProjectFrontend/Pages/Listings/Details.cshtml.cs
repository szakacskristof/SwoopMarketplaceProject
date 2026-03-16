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
        public bool CanEditOrDelete { get; private set; }

        public async Task<IActionResult> OnGetAsync(int azon)
        {
            if (azon <= 0)
               return NotFound();
            var lw = await _api.GetByAzonWithOwnerAsync(azon);
            if (lw is null || lw.Listing is null)
                return NotFound();
            Listing = lw.Listing;
            OwnerEmail = lw.OwnerEmail;
            // decide permission
            CanEditOrDelete = _auth.IsInRole("Admin") || string.Equals(_auth.GetEmail(), OwnerEmail, StringComparison.OrdinalIgnoreCase);
            return Page();
        }
    }
}
