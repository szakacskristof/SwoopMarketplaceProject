using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Dtos;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend.Pages.Admin
{
    [Authorize(Roles = "Admin,Owner,Tulaj")]
    public class IndexModel : PageModel
    {
        private readonly ListingApi _listingApi;
        private readonly UserApi _userApi;
        private readonly ReportApi _reportApi;
        private readonly AdminApi _adminApi;

        public IndexModel(ListingApi listingApi, UserApi userApi, ReportApi reportApi, AdminApi adminApi)
        {
            _listingApi = listingApi;
            _userApi = userApi;
            _reportApi = reportApi;
            _adminApi = adminApi;
        }

        [BindProperty(SupportsGet = true)]
        public string SelectedTab { get; set; } = "listings";

        public List<ListingWithOwnerDto>? Listings { get; set; }
        public List<UserDto>? Users { get; set; }
        public List<ReportDto>? Reports { get; set; }

        public async Task OnGetAsync()
        {
            // load everything (small project - acceptable)
            Listings = await _listingApi.GetAllWithOwnersAsync();
            Users = await _userApi.GetAllAsync();
            Reports = await _reportApi.GetAllAsync();

            // Enrich reports with reporter info (email/username).
            // The backend Reports API currently returns only the raw report record,
            // so we use the UserApi to look up the reporting user's public info.
            if (Reports?.Any() == true)
            {
                foreach (var r in Reports)
                {
                    try
                    {
                        var user = await _userApi.GetByAzonAsync(r.UserId);
                        if (user != null)
                        {
                            r.ReporterEmail = user.Email;
                            r.ReporterUsername = user.Username;
                        }
                    }
                    catch
                    {
                        // ignore enrichment errors to keep admin page usable
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteListingAsync(int azon)
        {
            try
            {
                await _listingApi.DeleteAsync(azon);
                TempData["Message"] = "Hirdetés törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "listings" });
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(long id)
        {
            try
            {
                await _userApi.DeleteAsync(id);
                TempData["Message"] = "Felhasználó törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message; 

            }
            return RedirectToPage(new { SelectedTab = "users" });
        }

        public async Task<IActionResult> OnPostDeleteReportAsync(long id)
        {
            try
            {
                await _reportApi.DeleteAsync(id);
                TempData["Message"] = "Report törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "reports" });
        }

        // NEW: delete both listing and its report
        public async Task<IActionResult> OnPostDeleteListingAndReportAsync(int listingId, long reportId)
        {
            try
            {
                // Delete listing first (requires auth header in ListingApi)
                await _listingApi.DeleteAsync(listingId);

                // Delete any report(s) referencing that listing.
                // Use new API that deletes by listing id to avoid NotFound when a single report id is stale.
                await _reportApi.DeleteByListingAsync(listingId);

                TempData["Message"] = "Hirdetés és report törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SelectedTab = "reports" });
        }

        // Csak Owner/Tulaj: szerep beállítása (frontend hívja a backend admin endpointot)
        public async Task<IActionResult> OnPostSetUserRoleAsync(long id, string role)
        {
            try
            {
                // Prevent assigning the 'Tulaj' role from the admin Razor page
                if (string.Equals(role, "Tulaj", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "A 'Tulaj' szerep az admin oldalon nem állítható.";
                }
                else
                {
                    await _adminApi.SetUserRoleAsync(id, role);
                    TempData["Message"] = "Szerep frissítve.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "users" });
        }
    }
}
