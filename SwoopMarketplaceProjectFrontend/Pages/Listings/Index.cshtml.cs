using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class IndexModel : PageModel
    {
        private readonly ListingApi _api;
        private readonly AuthSession _auth;
        private readonly CategoryApi _categoryApi;
        private readonly BookmarkApi _bookmarkApi;

        public List<ListingWithOwnerDto> ListingsWithOwners { get; private set; } = new();

        public List<CategoryDto> Categories { get; private set; } = new();

        // Selected category bound from query string (supports GET)
        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        // Location filter (town/city)
        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }

        // Price range filters
        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        // show saved/bookmarked view
        [BindProperty(SupportsGet = true)]
        public bool Saved { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? Error { get; set; }

        // Locations with counts (computed from listings)
        public record LocCount(string Name, int Count);
        public List<LocCount> Locations { get; private set; } = new();

        public IndexModel(ListingApi api, AuthSession auth, CategoryApi categoryApi, BookmarkApi bookmarkApi)
        {
            _api = api;
            _auth = auth;
            _categoryApi = categoryApi;
            _bookmarkApi = bookmarkApi;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // If the user requested the Saved tab but is not signed in -> redirect to login
            if (Saved && !_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Index", new { Saved = true }) });
            }

            // load categories for selector
            var cats = await _categoryApi.GetAllAsync();
            Categories = cats ?? new List<CategoryDto>();

            // load listings (with owners)
            var all = await _api.GetAllWithOwnersAsync();

            // compute location counts from the full set (before later filtering) so user sees all known locations
            var locGroups = all
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Listing.Location) ? "Ismeretlen" : x.Listing.Location!.Trim())
                .Select(g => new LocCount(g.Key, g.Count()))
                .OrderByDescending(l => l.Count)
                .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Locations = locGroups;

            // mark bookmarks if signed in
            HashSet<long>? bookmarked = null;
            if (_auth.IsSignedIn)
            {
                try
                {
                    var ids = await _bookmarkApi.GetForCurrentUserAsync();
                    bookmarked = new HashSet<long>(ids);
                }
                catch { bookmarked = null; }
            }

            // Start from full list and apply filters
            var filtered = all.AsEnumerable();

            if (CategoryId.HasValue)
            {
                filtered = filtered.Where(x => x.Listing.CategoryId.HasValue && x.Listing.CategoryId.Value == CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(Location))
            {
                var locTrim = Location.Trim();
                filtered = filtered.Where(x => string.Equals((x.Listing.Location ?? "").Trim(), locTrim, StringComparison.OrdinalIgnoreCase));
            }

            if (MinPrice.HasValue)
            {
                filtered = filtered.Where(x => x.Listing.Price >= MinPrice.Value);
            }

            if (MaxPrice.HasValue)
            {
                filtered = filtered.Where(x => x.Listing.Price <= MaxPrice.Value);
            }

            ListingsWithOwners = filtered.ToList();

            // annotate bookmark state
            if (bookmarked != null)
            {
                foreach (var l in ListingsWithOwners)
                    l.IsBookmarked = bookmarked.Contains(l.Listing.Id);
            }
            else
            {
                foreach (var l in ListingsWithOwners)
                    l.IsBookmarked = false;
            }

            // if saved tab requested, filter to bookmarked only
            if (Saved)
            {
                ListingsWithOwners = ListingsWithOwners.Where(x => x.IsBookmarked).ToList();
            }

            return Page();
        }

        // Handler invoked by the inline delete form (onsubmit uses confirm())
        public async Task<IActionResult> OnPostDeleteAsync(int azon)
        {
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Index") });
            }

            // Re-load the listing with owner info to validate ownership on server side
            var lwo = await _api.GetByAzonWithOwnerAsync(azon);
            if (lwo is null)
            {
                Error = "Hirdetés nem található!";
                return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
            }

            var ownerEmail = lwo.OwnerEmail;
            var callerEmail = _auth.GetEmail();

            if (!(_auth.IsInRole("Admin") || string.Equals(callerEmail, ownerEmail, StringComparison.OrdinalIgnoreCase)))
            {
                Error = "A felhasználó nincs azonosítva a hirdetés törléséhez!";
                return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
            }

            try
            {
                await _api.DeleteAsync(azon);
                Message = "Hirdetés törölve!";
                return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
                return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
            }
        }

        // toggles bookmark on a listing (POST)
        public async Task<IActionResult> OnPostToggleBookmarkAsync(long azon)
        {
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Index", new { CategoryId, Location, MinPrice, MaxPrice, Saved }) });
            }

            try
            {
                // figure out current state from server
                var ids = await _bookmarkApi.GetForCurrentUserAsync();
                if (ids.Contains(azon))
                {
                    await _bookmarkApi.RemoveAsync(azon);
                }
                else
                {
                    await _bookmarkApi.AddAsync(azon);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return RedirectToPage(new { CategoryId, Location, MinPrice, MaxPrice, Saved });
        }
    }
}
