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

        public List<ListingWithOwnerDto>? Listings { get; set; }
        public List<UserDto>? Users { get; set; }
        public List<ReportDto>? Reports { get; set; }

        // OnGetAsync: load listings, users and reports for admin dashboard.
        public async Task OnGetAsync()
        {
            Listings = await _listingApi.GetAllWithOwnersAsync();
            Users = await _userApi.GetAllAsync();
            Reports = await _reportApi.GetAllAsync();

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
        }

        // OnPostDeleteListingAsync: admin action to delete a listing.
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

        // OnPostDeleteUserAsync: admin action to delete a user with role protection checks.
        public async Task<IActionResult> OnPostDeleteUserAsync(long id)
        {
            try
            {
                var targetUser = await _userApi.GetByAzonAsync(id);

                if (targetUser == null)
                {
                    TempData["Error"] = "A felhasználó nem található.";
                    return RedirectToPage(new { SelectedTab = "users" });
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
                    TempData["Error"] = "Admin nem törölhet Admin, Tulaj vagy Owner szerepkörû felhasználót.";
                    return RedirectToPage(new { SelectedTab = "users" });
                }

                await _userApi.DeleteAsync(id);
                TempData["Message"] = "Felhasználó törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SelectedTab = "users" });
        }

        // OnPostDeleteReportAsync: delete a report entry.
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

        // OnPostDeleteListingAndReportAsync: delete both listing and its associated report.
        public async Task<IActionResult> OnPostDeleteListingAndReportAsync(int listingId, long reportId)
        {
            try
            {
                await _listingApi.DeleteAsync(listingId);
                await _reportApi.DeleteByListingAsync(listingId);
                TempData["Message"] = "Hirdetés és report törölve.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { SelectedTab = "reports" });
        }

        // OnPostSetUserRoleAsync: set a user's role via admin API (with safeguards).
        public async Task<IActionResult> OnPostSetUserRoleAsync(long id, string role)
        {
            try
            {
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
