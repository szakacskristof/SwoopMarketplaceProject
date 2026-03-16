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


        public IndexModel(ListingApi api, AuthSession auth) { _api = api; _auth = auth; }


        public async Task OnGetAsync()

        {

            ListingsWithOwners = await _api.GetAllWithOwnersAsync();

        }

    }
}
