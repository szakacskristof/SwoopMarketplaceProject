using Microsoft.AspNetCore.Mvc.RazorPages;

using SwoopMarketplaceProjectFrontend.Dtos;

using SwoopMarketplaceProjectFrontend.Services;


namespace SwoopMarketplaceProjectFrontend.Pages.Listings

{

    public class IndexModel : PageModel

    {

        private readonly ListingApi _api;


        public List<ListingDto> Listings { get; private set; } = new();


        public IndexModel(ListingApi api) => _api = api;


        public async Task OnGetAsync()

        {

            Listings = await _api.GetAllAsync();

        }

    }
}
