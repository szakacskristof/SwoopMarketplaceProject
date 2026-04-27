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
        private readonly AuthSession _authSession;

        public IndexModel(ListingApi listingApi, UserApi userApi, ReportApi reportApi, AdminApi adminApi, AuthSession authSession)
        {
            _listingApi = listingApi;
            _userApi = userApi;
            _reportApi = reportApi;
            _adminApi = adminApi;
            _authSession = authSession;
        }

        // Constructor: initialize admin page with necessary APIs and auth session.

        [BindProperty(SupportsGet = true)]
        public string SelectedTab { get; set; } = "listings";

        [BindProperty(SupportsGet = true)]
        public string? UserSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ListingSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReportSearch { get; set; }

        public List<ListingWithOwnerDto>? Listings { get; set; }
        public List<UserDto>? Users { get; set; }
        public List<ReportDto>? Reports { get; set; }

        // OnGetAsync: load listings, users and reports for admin dashboard.
        public async Task OnGetAsync()
        {
            Listings = await _listingApi.GetAllWithOwnersAsync();
            Users = await _userApi.GetAllAsync();
            Reports = await _reportApi.GetAllAsync();

            // Apply user search filter (case-insensitive) when provided
            if (!string.IsNullOrWhiteSpace(UserSearch) && Users != null)
            {
                var q = UserSearch.Trim();
                Users = Users
                    .Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && u.Username.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(u.Email) && u.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }

            // Enrich reports with reporter info first so filtering can use reporter fields
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
                    }
                }
            }

            // Apply listing search (search title, owner email, description, category)
            if (!string.IsNullOrWhiteSpace(ListingSearch) && Listings != null)
            {
                var q = ListingSearch.Trim();
                Listings = Listings
                    .Where(l =>
                        (!string.IsNullOrEmpty(l.Listing?.Title) && l.Listing.Title.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(l.OwnerEmail) && l.OwnerEmail.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(l.Listing?.Description) && l.Listing.Description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(l.Listing?.CategoryName) && l.Listing.CategoryName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }

            // Apply report search (search description, reporter username or email, listing id)
            if (!string.IsNullOrWhiteSpace(ReportSearch) && Reports != null)
            {
                var q = ReportSearch.Trim();
                Reports = Reports
                    .Where(r =>
                        (!string.IsNullOrEmpty(r.Description) && r.Description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(r.ReporterUsername) && r.ReporterUsername.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(r.ReporterEmail) && r.ReporterEmail.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (long.TryParse(q, out var lid) && r.ListingId == lid))
                    .ToList();
            }
        }

        // OnPostDeleteListingAsync: admin action to delete a listing.
        public async Task<IActionResult> OnPostDeleteListingAsync(int azon)
        {
            try
            {
                await _listingApi.DeleteAsync(azon);
                TempData["Message"] = "Hirdet�s t�r�lve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "listings", ListingSearch });
        }

        // OnPostDeleteUserAsync: admin action to delete a user with role protection checks.
        public async Task<IActionResult> OnPostDeleteUserAsync(long id)
        {
            try
            {
                var targetUser = await _userApi.GetByAzonAsync(id);

                if (targetUser == null)
                {
                    TempData["Error"] = "A felhaszn�l� nem tal�lhat�.";
                    return RedirectToPage(new { SelectedTab = "users", UserSearch });
                }

                var targetRoles = targetUser.Roles ?? new List<string>();
                var targetIsProtected = targetRoles.Any(r =>
                    string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, "Tulaj", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, "Owner", StringComparison.OrdinalIgnoreCase));

                var currentIsAdminOnly =
                    _authSession.IsInRole("Admin") &&
                    !_authSession.IsInRole("Tulaj") &&
                    !_authSession.IsInRole("Owner");

                if (currentIsAdminOnly && targetIsProtected)
                {
                    TempData["Error"] = "Admin nem t�r�lhet Admin, Tulaj vagy Owner szerepk�r� felhaszn�l�t.";
                    return RedirectToPage(new { SelectedTab = "users", UserSearch });
                }

                await _userApi.DeleteAsync(id);
                TempData["Message"] = "Felhaszn�l� t�r�lve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SelectedTab = "users", UserSearch });
        }

        // OnPostDeleteReportAsync: delete a report entry.
        public async Task<IActionResult> OnPostDeleteReportAsync(long id)
        {
            try
            {
                await _reportApi.DeleteAsync(id);
                TempData["Message"] = "Report t�r�lve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "reports", ReportSearch });
        }

        // OnPostDeleteListingAndReportAsync: delete both listing and its associated report.
        public async Task<IActionResult> OnPostDeleteListingAndReportAsync(int listingId, long reportId)
        {
            try
            {
                await _listingApi.DeleteAsync(listingId);
                await _reportApi.DeleteByListingAsync(listingId);
                TempData["Message"] = "Hirdet�s �s report t�r�lve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SelectedTab = "reports", ReportSearch });
        }

        // OnPostSetUserRoleAsync: set a user's role via admin API (with safeguards).
        public async Task<IActionResult> OnPostSetUserRoleAsync(long id, string role)
        {
            try
            {
                if (string.Equals(role, "Tulaj", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "A 'Tulaj' szerep az admin oldalon nem �ll�that�.";
                }
                else
                {
                    await _adminApi.SetUserRoleAsync(id, role);
                    TempData["Message"] = "Szerep friss�tve.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { SelectedTab = "users", UserSearch });
        }
    }
}
