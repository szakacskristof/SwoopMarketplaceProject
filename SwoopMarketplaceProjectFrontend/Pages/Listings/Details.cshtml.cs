using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class DetailsModel : PageModel
    {
        private readonly ListingApi _api;
        private readonly AuthSession _auth;
        public DetailsModel(ListingApi api, AuthSession auth)
        {
            _api = api;
            _auth = auth;
        }
        public ListingDto? Listing { get; private set; }

        public async Task<IActionResult> OnGetAsync(int azon)
        {
            if (azon <= 0)
               return NotFound();
            Listing = await _api.GetByAzonAsync(azon);
            if (Listing is null)
                return NotFound();
            return Page();
        }
    }
}
