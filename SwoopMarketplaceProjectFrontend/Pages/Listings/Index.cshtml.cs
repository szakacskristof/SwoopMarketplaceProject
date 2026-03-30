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

        public List<ListingWithOwnerDto> ListingsWithOwners { get; private set; } = new();

        public List<CategoryDto> Categories { get; private set; } = new();

        // Selected category bound from query string (supports GET)
        [BindProperty(SupportsGet = true)]
        public long? CategoryId { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? Error { get; set; }

        public IndexModel(ListingApi api, AuthSession auth, CategoryApi categoryApi)
        {
            _api = api;
            _auth = auth;
            _categoryApi = categoryApi;
        }

        public async Task OnGetAsync()
        {
            // load categories for selector
            var cats = await _categoryApi.GetAllAsync();
            Categories = cats ?? new List<CategoryDto>();

            // load listings (with owners)
            var all = await _api.GetAllWithOwnersAsync();

            if (CategoryId.HasValue)
            {
                ListingsWithOwners = all
                    .Where(x => x.Listing.CategoryId.HasValue && x.Listing.CategoryId.Value == CategoryId.Value)
                    .ToList();
            }
            else
            {
                ListingsWithOwners = all.ToList();
            }
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
                Error = "Hírdetés nem található!";
                return RedirectToPage(new { CategoryId });
            }

            var ownerEmail = lwo.OwnerEmail;
            var callerEmail = _auth.GetEmail();

            if (!(_auth.IsInRole("Admin") || string.Equals(callerEmail, ownerEmail, StringComparison.OrdinalIgnoreCase)))
            {
                Error = "A fiók nincs azonosítva a hírdetés törléséhez!";
                return RedirectToPage(new { CategoryId });
            }

            try
            {
                await _api.DeleteAsync(azon);
                Message = "Hírdetés törölve!";
                return RedirectToPage(new { CategoryId });
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
                return RedirectToPage(new { CategoryId });
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return RedirectToPage(new { CategoryId });
            }
        }
    }
}
