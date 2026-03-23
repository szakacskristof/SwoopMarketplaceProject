using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class EditModel : PageModel
    {
        private readonly ListingApi _listingApi;
        private readonly CategoryApi? _categoryApi;
        private readonly ListingImageApi _listingImageApi;
        private readonly AuthSession _auth;

        public EditModel(ListingApi listingApi, ListingImageApi listingImageApi, AuthSession auth, CategoryApi? categoryApi = null)
        {
            _listingApi = listingApi;
            _listingImageApi = listingImageApi;
            _auth = auth;
            _categoryApi = categoryApi;
        }

        public record CategoryItem(long Id, string Name);

        public IList<CategoryItem> Categories { get; private set; } = new List<CategoryItem>();

        [BindProperty]
        public ListingDto? Input { get; set; }

        public string? Message { get; set; }
        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync(int azon)
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Listings/Edit", new { azon }) });

            await LoadCategoriesAsync();

            var listing = await _listingApi.GetByAzonAsync(azon);
            if (listing is null)
                return NotFound();

            Input = listing;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input is null)
                return BadRequest();

            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            await LoadCategoriesAsync();

            if (!ModelState.IsValid)
            {
                Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)));
                return Page();
            }

            // ensure id present
            if (Input.Id == 0)
            {
                Error = "Missing listing id.";
                return Page();
            }

            try
            {
                // call API to update listing
                await _listingApi.UpdateAsync((int)Input.Id, Input);

                // upload any new images posted with the form
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
                            ListingId = Input.Id,
                            ImageUrl = dataUrl,
                            IsPrimary = null
                        };

                        try
                        {
                            await _listingImageApi.CreateAsync(imgDto);
                        }
                        catch (Exception ex)
                        {
                            // non-fatal: collect error to show to user
                            ModelState.AddModelError(string.Empty, $"Image upload failed: {ex.Message}" );
                        }
                    }
                }

                // redirect to details page after successful edit
                return RedirectToPage("/Listings/Details", new { azon = Input.Id });
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
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
        }
    }
}
