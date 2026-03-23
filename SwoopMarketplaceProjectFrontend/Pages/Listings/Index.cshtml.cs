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


        public List<ListingWithOwnerDto> ListingsWithOwners { get; private set; } = new();


        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? Error { get; set; }


        public IndexModel(ListingApi api, AuthSession auth) { _api = api; _auth = auth; }


        public async Task OnGetAsync()

        {

            ListingsWithOwners = await _api.GetAllWithOwnersAsync();

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

                Error = "Listing not found.";

                return RedirectToPage();

            }


            var ownerEmail = lwo.OwnerEmail;

            var callerEmail = _auth.GetEmail();


            if (!(_auth.IsInRole("Admin") || string.Equals(callerEmail, ownerEmail, StringComparison.OrdinalIgnoreCase)))

            {

                Error = "You are not authorized to delete this listing.";

                return RedirectToPage();

            }

            try

            {

                await _api.DeleteAsync(azon);

                Message = "Listing deleted.";

                return RedirectToPage();

            }

            catch (HttpRequestException ex)

            {

                Error = ex.Message;

                return RedirectToPage();

            }

            catch (Exception ex)

            {

                Error = ex.Message;

                return RedirectToPage();

            }

        }

    }

}
