using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SwoopMarketplaceProject.Models;

namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SwoopContext _context;

        public AdminController(UserManager<IdentityUser> userManager, SwoopContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Allow both role name variants used across the app
        [HttpPost("users/{id}/set-role")]
        [Authorize(Roles = "Tulaj,Owner")]
        public async Task<IActionResult> SetUserRole(long id, [FromBody] RoleRequest req)
        {
            var appUser = await _context.Users.FindAsync(id);
            if (appUser == null) return NotFound("Nem találtuk a felhasználót.");

            if (string.IsNullOrWhiteSpace(appUser.Email))
                return BadRequest("Felhasználó email nem található.");

            var identityUser = await _userManager.FindByEmailAsync(appUser.Email);
            if (identityUser == null) return NotFound("Identity felhasználó nem található.");

            // remove all current roles and add requested role
            var currentRoles = await _userManager.GetRolesAsync(identityUser);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                if (!removeResult.Succeeded)
                    return StatusCode(500, "Nem sikerült a rang elvétele.");
            }

            var addResult = await _userManager.AddToRoleAsync(identityUser, req.Role);
            if (!addResult.Succeeded)
            {
                var msg = string.Join("; ", addResult.Errors.Select(e => e.Description));
                return StatusCode(500, $"Nem sikerült a rang hozzáadása:{msg}");
            }

            return NoContent();
        }

        public class RoleRequest { public string Role { get; set; } = "User"; }
    }
}
