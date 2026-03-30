using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Listings
{
    public class EditModel : PageModel
    {
        private readonly ListingApi _listingApi;
        private readonly CategoryApi? _category_api;
        private readonly ListingImageApi _listing_image_api;
        private readonly AuthSession _auth;

        public EditModel(ListingApi listingApi, ListingImageApi listingImageApi, AuthSession auth, CategoryApi? categoryApi = null)
        {
            _listingApi = listingApi;
            _listing_image_api = listingImageApi;
            _auth = auth;
            _category_api = categoryApi;
        }

        public record CategoryItem(long Id, string Name);

        public IList<CategoryItem> Categories { get; private set; } = new List<CategoryItem>();

        [BindProperty]
        public ListingDto? Input { get; set; }

        // existing images for the listing (used by the view)
        public List<ListingImageDto> ExistingImages { get; set; } = new();

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

            // surface redirect messages (upload success / delete errors)
            Message = TempData["Message"] as string;
            if (TempData["ImageDeleteError"] != null)
                Error = TempData["ImageDeleteError"] as string;

            await LoadExistingImagesAsync(listing.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input is null)
                return BadRequest();

            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            await LoadCategoriesAsync();

            // ensure ExistingImages loaded so page can re-render if validation fails
            await LoadExistingImagesAsync(Input.Id);

            if (!ModelState.IsValid)
            {
                Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)));
                return Page();
            }

            // ensure id present
            if (Input.Id == 0)
            {
                Error = "Hiányzó hirdetés ID";
                return Page();
            }

            try
            {
                // update listing
                await _listingApi.UpdateAsync((int)Input.Id, Input);

                // upload any new images posted with the form using multipart upload (same as Create page)
                var files = Request.Form?.Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length == 0) continue;

                        try
                        {
                            // now UploadAsync returns created DTO - we ignore return here
                            await _listing_image_api.UploadAsync(Input.Id, file);
                        }
                        catch (Exception ex)
                        {
                            // non-fatal: collect error to show to user
                            ModelState.AddModelError(string.Empty, $"Kép feltöltése sikertelen!: {ex.Message}");
                        }
                    }
                }

                // primary image selection (copied from client into hidden input primaryImageId)
                var primaryVal = Request.Form["primaryImageId"].FirstOrDefault();
                if (long.TryParse(primaryVal, out var primaryId) && primaryId > 0)
                {
                    try
                    {
                        await _listing_image_api.SetPrimaryAsync(Input.Id, primaryId);
                    }
                    catch
                    {
                        // ignore primary set failures (non-fatal), but record message
                        TempData["ImageDeleteError"] = "Megjelenõ kép kiválasztása sikertelen!";
                    }
                }

                TempData["Message"] = "Listing saved.";
                return RedirectToPage("/Listings/Edit", new { azon = Input.Id });
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

        // delete a listing image and go back to the edit page
        public async Task<IActionResult> OnPostDeleteImageAsync(int id, int listingId)
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            try
            {
                await _listing_image_api.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                TempData["ImageDeleteError"] = ex.Message;
            }

            return RedirectToPage("/Listings/Edit", new { azon = listingId });
        }

        private async Task LoadCategoriesAsync()
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
        }

        private async Task LoadExistingImagesAsync(long listingId)
        {
            ExistingImages.Clear();
            try
            {
                // fetch DB-backed listing images (gives us ids)
                var all = await _listing_image_api.GetAllAsync();
                if (all == null)
                    return;

                var imagesForListing = all.Where(i => i.ListingId == listingId).ToList();

                // Build filename -> absolute URL map from ListingDto.ImageUrls (Listings API typically returns absolute URLs)
                var filenameToFullUrl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (Input?.ImageUrls != null)
                {
                    foreach (var full in Input.ImageUrls)
                    {
                        if (string.IsNullOrWhiteSpace(full)) continue;
                        try
                        {
                            var uri = new Uri(full, UriKind.RelativeOrAbsolute);
                            var local = uri.IsAbsoluteUri ? uri.LocalPath : full;
                            var name = Path.GetFileName(local);
                            if (!string.IsNullOrWhiteSpace(name) && !filenameToFullUrl.ContainsKey(name))
                                filenameToFullUrl[name] = full;
                        }
                        catch
                        {
                            // ignore malformed URL
                        }
                    }
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}".TrimEnd('/');

                ExistingImages = imagesForListing.Select(i =>
                {
                    var img = new ListingImageDto
                    {
                        Id = i.Id,
                        ListingId = i.ListingId,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary
                    };

                    // try to match a full absolute URL from Input.ImageUrls by filename
                    try
                    {
                        var fileName = Path.GetFileName(img.ImageUrl ?? string.Empty);
                        if (!string.IsNullOrWhiteSpace(fileName) && filenameToFullUrl.TryGetValue(fileName, out var matchedFull))
                        {
                            img.ImageUrl = matchedFull;
                            return img;
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    // leave absolute urls as-is
                    if (!string.IsNullOrWhiteSpace(img.ImageUrl) && Uri.IsWellFormedUriString(img.ImageUrl, UriKind.Absolute))
                        return img;

                    // keep data: urls
                    if (!string.IsNullOrWhiteSpace(img.ImageUrl) && img.ImageUrl.StartsWith("data:"))
                        return img;

                    // Otherwise, normalize relative URL using current host
                    if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                    {
                        var u = img.ImageUrl.TrimStart('~').TrimStart('/');
                        img.ImageUrl = $"{baseUrl}/{u}";
                    }

                    return img;
                })
                .OrderByDescending(i => i.IsPrimary) // primary first
                .ToList();
            }
            catch
            {
                // ignore - leave ExistingImages empty if API call fails
            }
        }
    }
}
