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
        private readonly CategoryApi? _categoryApi; // optional
        private readonly ListingImageApi _listingImageApi;
        private readonly AuthSession _auth;
        private readonly UserApi _userApi;

        public CreateModel(ListingApi listingApi, ListingImageApi listingImageApi, AuthSession auth, UserApi userApi, CategoryApi? categoryApi = null)
        {
            _listingApi = listingApi;
            _listingImageApi = listingImageApi;
            _auth = auth;
            _userApi = userApi;
            _categoryApi = categoryApi;
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

            [Required(ErrorMessage = "Please select a category.")]
            public long? CategoryId { get; set; }

            public string Condition { get; set; } = "used";

            public string? Location { get; set; }

            [Required]
            public string Description { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories.Clear();
            if (_categoryApi != null)
            {
                var cats = await _categoryApi.GetAllAsync();
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
                ModelState.AddModelError(string.Empty, "Your account is missing the required role ('User' or 'Admin'). The API requires one of these roles to create listings.");
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
                        ModelState.AddModelError(string.Empty, "Current user profile not found; cannot create listing.");
                        return Page();
                    }

                    var localPart = email.Split('@').FirstOrDefault() ?? "user";
                    user = new UserDto
                    {
                        Id = 0,
                        Username = localPart,
                        Email = email,
                        Phone = "",
                        // backend only validates presence of PasswordHash here; store a harmless placeholder
                        
                        ProfileImageUrl = null,
                        Bio = null,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                var created = await _listingApi.CreateAsync(dto);
                if (created == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create listing.");
                    return Page();
                }

                // handle image uploads from form files: convert to base64 data-URI and post to ListingImages API
                var files = Request.Form?.Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length == 0) continue;
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        var base64 = Convert.ToBase64String(bytes);
                        var dataUrl = $"data:{file.ContentType};base64,{base64}";

                        var imgDto = new ListingImageDto
                        {
                            ListingId = created.Id,
                            ImageUrl = dataUrl,
                            IsPrimary = null
                        };

                        try
                        {
                            await _listingImageApi.CreateAsync(imgDto);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, $"Image upload failed: {ex.Message}");
                        }
                    }
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
