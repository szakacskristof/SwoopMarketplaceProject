using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class CreateModel : PageModel
    {
        private readonly ListingApi _listingApi;
        private readonly CategoryApi? _category_api;
        private readonly ListingImageApi _listing_image_api;
        private readonly AuthSession _auth;
        private readonly UserApi _userApi;

        public CreateModel(ListingApi listingApi, ListingImageApi listingImageApi, AuthSession auth, UserApi userApi, CategoryApi? categoryApi = null)
        {
            _listingApi = listingApi;
            _listing_image_api = listingImageApi;
            _auth = auth;
            _userApi = userApi;
            _category_api = categoryApi;
        }

        public record CategoryItem(long Id, string Name);

        public IList<CategoryItem> Categories { get; private set; } = new List<CategoryItem>();

        // Debug properties (read-only) exposed to the view
        public string? DebugToken { get; private set; }
        public string? DebugEmail { get; private set; }
        public long? DebugUserId { get; private set; }
        public IReadOnlyList<string> DebugRoles { get; private set; } = Array.Empty<string>();

        public class InputModel
        {
            [Required]
            public string Title { get; set; } = string.Empty;

            public decimal Price { get; set; }

            [Required(ErrorMessage = "Kérlek válassz kategóriát!")]
            public long? CategoryId { get; set; }

            public string Condition { get; set; } = "Használt";

            public string? Location { get; set; }

            [Required]
            public string Description { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories.Clear();
            if (_category_api != null)
            {
                var cats = await _category_api.GetAllAsync();
                if (cats != null)
                {
                    foreach (var c in cats)
                        Categories.Add(new CategoryItem(c.Id, c.Name));
                }
            }

            // populate debug values for view
            DebugToken = _auth.GetToken();
            DebugEmail = _auth.GetEmail();
            DebugUserId = _auth.GetUserId();
            DebugRoles = _auth.GetRoles();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // require authentication for creating (backend requires Auth)
            if (!_auth.IsSignedIn)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Create") });
            }

            await OnGetAsync(); // reload categories and debug values for redisplay

            if (!ModelState.IsValid)
                return Page();

            // quick pre-check: ensure JWT contains a required role (User or Admin)
            var roles = _auth.GetRoles();
            if (!roles.Any(r => r.Equals("User", StringComparison.OrdinalIgnoreCase) || r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(string.Empty, "A fiókod nem rendelkezik az adott jogosultságokkal. A hírdetés feladásához Admin/Felhasználó jogosultság szükséges!");
                return Page();
            }

            var dto = new ListingDto
            {
                Title = Input.Title,
                Description = Input.Description,
                Price = Input.Price,
                CategoryId = Input.CategoryId,
                Condition = Input.Condition,
                Location = Input.Location,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Try to fetch current full user profile by user id from JWT first, then fallback to email
                UserDto? user = null;
                var uid = _auth.GetUserId();
                if (uid.HasValue)
                {
                    // API expects int id parameter
                    user = await _userApi.GetByAzonAsync((int)uid.Value);
                }

                if (user is null)
                {
                    var email = _auth.GetEmail();
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        user = await _userApi.GetByEmailAsync(email);
                    }
                }

                // If still null, synthesize a minimal user payload from JWT claims (fallback)
                if (user is null)
                {
                    var email = _auth.GetEmail();
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        ModelState.AddModelError(string.Empty, "A felhasználói profil nem található, nem hozható létre hírdetés!");
                        return Page();
                    }

                    var localPart = email.Split('@').FirstOrDefault() ?? "user";
                    user = new UserDto
                    {
                        Id = 0,
                        Username = localPart,
                        Email = email,
                        Phone = "",
                        ProfileImageUrl = null,
                        Bio = null,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                var created = await _listingApi.CreateAsync(dto);
                if (created == null)
                {
                    ModelState.AddModelError(string.Empty, "Nem sikerült a hírdetés feladása.");
                    return Page();
                }

                // handle image uploads from form files: upload via multipart/form-data to API
                var files = Request.Form?.Files;
                var createdImages = new List<ListingImageDto>();
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length == 0) continue;

                        try
                        {
                            var ci = await _listing_image_api.UploadAsync(created.Id, file);
                            if (ci != null && ci.Id > 0)
                                createdImages.Add(ci);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, $"Kép feltöltése sikertelen!: {ex.Message}");
                        }
                    }
                }

                // Primary selection: check both the hidden index and the radio-group name (safety)
                var primaryIndexStr = Request.Form["primaryFileIndex"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(primaryIndexStr))
                    primaryIndexStr = Request.Form["primarySelectorCreate"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(primaryIndexStr) && int.TryParse(primaryIndexStr, out var idx))
                {
                    if (idx >= 0 && idx < createdImages.Count)
                    {
                        try
                        {
                            await _listing_image_api.SetPrimaryAsync(created.Id, createdImages[idx].Id);
                        }
                        catch (Exception ex)
                        {
                            // Provide useful diagnostics to the user if SetPrimary fails
                            ModelState.AddModelError(string.Empty, $"Megjelenõ kép kiválasztása sikertelen: {ex.Message}");
                            // Show the page so user sees the error and can retry
                            return Page();
                        }
                    }
                    else
                    {
                        // Index out of range — show diagnostics so you can see what's being posted
                        ModelState.AddModelError(string.Empty, $"A kiválasztott kép {idx} nincs benne a feltöltött képek listában (0..{Math.Max(0, createdImages.Count - 1)}). feltöltött képek: {string.Join(", ", createdImages.Select(ci => ci.Id))}");
                        return Page();
                    }
                }
                else
                {
                    // No explicit primary chosen — keep server default (first uploaded becomes primary) but inform user in debug mode
                    // (no error; proceed)
                }

                return RedirectToPage("/Listings/Details", new { azon = created.Id });
            }
            catch (HttpRequestException ex)
            {
                // add helpful guidance when a 403 occurs
                if (ex.Message.Contains("403"))
                {
                    ModelState.AddModelError(string.Empty, "API returned 403 Forbidden. Common causes: missing/expired token, or JWT lacks the 'User'/'Admin' role. Ensure you are signed in and your account has the 'User' role (ask admin).");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                return Page();
            }
        }
    }
}
