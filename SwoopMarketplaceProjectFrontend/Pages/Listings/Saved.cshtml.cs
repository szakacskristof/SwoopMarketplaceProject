using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class SavedModel : PageModel
    {
        private readonly ListingApi _api;
        private readonly BookmarkApi _bookmarkApi;
        private readonly AuthSession _auth;

        public List<ListingWithOwnerDto> ListingsWithOwners { get; private set; } = new();

        public SavedModel(ListingApi api, BookmarkApi bookmarkApi, AuthSession auth)
        {
            _api = api;
            _bookmarkApi = bookmarkApi;
            _auth = auth;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Saved") });
            }

            // fetch bookmarked IDs then get listings and filter
            var ids = await _bookmarkApi.GetForCurrentUserAsync();
            var all = await _api.GetAllWithOwnersAsync();

            ListingsWithOwners = all
                .Where(x => ids.Contains(x.Listing.Id))
                .ToList();

            // annotate for UI
            foreach (var l in ListingsWithOwners)
                l.IsBookmarked = true;

            return Page();
        }

        // Toggle bookmark (keeps you on the Saved page)
        public async Task<IActionResult> OnPostToggleBookmarkAsync(long azon)
        {
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Saved") });
            }

            try
            {
                var ids = await _bookmarkApi.GetForCurrentUserAsync();
                if (ids.Contains(azon))
                    await _bookmarkApi.RemoveAsync(azon);
                else
                    await _bookmarkApi.AddAsync(azon);
            }
            catch
            {
                // ignore salt-of-the-earth errors; indexing UI will reflect current server state next load
            }

            return RedirectToPage("/Listings/Saved");
        }
    }
}
