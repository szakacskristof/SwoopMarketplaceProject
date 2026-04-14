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
            if (appUser == null) return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(appUser.Email))
                return BadRequest("User email missing (cannot map to identity user).");

            var identityUser = await _userManager.FindByEmailAsync(appUser.Email);
            if (identityUser == null) return NotFound("Identity user not found.");

            // remove all current roles and add requested role
            var currentRoles = await _userManager.GetRolesAsync(identityUser);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                if (!removeResult.Succeeded)
                    return StatusCode(500, "Unable to remove existing roles.");
            }

            var addResult = await _userManager.AddToRoleAsync(identityUser, req.Role);
            if (!addResult.Succeeded)
            {
                var msg = string.Join("; ", addResult.Errors.Select(e => e.Description));
                return StatusCode(500, $"Unable to add role: {msg}");
            }

            return NoContent();
        }

        public class RoleRequest { public string Role { get; set; } = "User"; }
    }
}
