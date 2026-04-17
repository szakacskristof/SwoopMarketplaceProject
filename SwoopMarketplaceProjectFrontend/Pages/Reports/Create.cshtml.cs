using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Reports
{
    public class CreateModel : PageModel
    {
        private readonly ListingApi _listingApi;
        private readonly ReportApi _reportApi;
        private readonly AuthSession _auth;
        private readonly UserApi _userApi;

        public CreateModel(ListingApi listingApi, ReportApi reportApi, AuthSession auth, UserApi userApi)
        {
            _listingApi = listingApi;
            _reportApi = reportApi;
            _auth = auth;
            _userApi = userApi;
        }

        [BindProperty(SupportsGet = true)]
        public long ListingId { get; set; }

        public ListingDto? Listing { get; private set; }

        public class InputModel
        {
            [Required]
            [StringLength(2000, MinimumLength = 10, ErrorMessage = "Kérjük, írj legalább 10 karaktert.")]
            public string Description { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (ListingId <= 0)
            {
                return NotFound();
            }

            Listing = await _listingApi.GetByAzonAsync((int)ListingId);
            if (Listing is null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ListingId <= 0)
            {
                return NotFound();
            }

            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new
                {
                    returnUrl = Url.Page("/Reports/Create", new { listingId = ListingId })
                });
            }

            Listing = await _listingApi.GetByAzonAsync((int)ListingId);
            if (Listing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _auth.GetUserId();
            if (!userId.HasValue)
            {
                var email = _auth.GetEmail();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var user = await _userApi.GetByEmailAsync(email);
                    userId = user?.Id;
                }
            }

            if (!userId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Nem sikerült azonosítani a jelenlegi felhasználót.");
                return Page();
            }

            var report = new ReportDto
            {
                ListingId = ListingId,
                UserId = userId.Value,
                Description = Input.Description
            };

            await _reportApi.CreateAsync(report);

            return RedirectToPage("/Listings/Details", new { azon = ListingId });
        }
    }
}
