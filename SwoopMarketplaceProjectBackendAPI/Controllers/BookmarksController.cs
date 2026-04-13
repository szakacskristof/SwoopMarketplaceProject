using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class BookmarksController : ControllerBase
    {
        private readonly SwoopContext _context;

        public BookmarksController(SwoopContext context)
        {
            _context = context;
        }

        // GET: api/Bookmarks
        // Returns list of listingIds bookmarked by the current user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<long>>> GetForCurrentUser()
        {
            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (appUser is null)
                return Forbid();

            var ids = await _context.Bookmarks
                .Where(b => b.UserId == appUser.Id)
                .Select(b => b.ListingId)
                .ToListAsync();

            return Ok(ids);
        }

        // POST: api/Bookmarks
        // Body: { listingId: long }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddBookmarkRequest req)
        {
            if (req == null || req.ListingId <= 0)
                return BadRequest();

            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (appUser is null)
                return Forbid();

            var exists = await _context.Bookmarks.AnyAsync(b => b.UserId == appUser.Id && b.ListingId == req.ListingId);
            if (!exists)
            {
                var bm = new Bookmark { UserId = appUser.Id, ListingId = req.ListingId, CreatedAt = DateTime.UtcNow };
                _context.Bookmarks.Add(bm);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // DELETE: api/Bookmarks/{listingId}
        [HttpDelete("{listingId:long}")]
        public async Task<IActionResult> Remove(long listingId)
        {
            if (listingId <= 0) return BadRequest();

            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (appUser is null)
                return Forbid();

            var bm = await _context.Bookmarks.FirstOrDefaultAsync(b => b.UserId == appUser.Id && b.ListingId == listingId);
            if (bm != null)
            {
                _context.Bookmarks.Remove(bm);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        public class AddBookmarkRequest
        {
            public long ListingId { get; set; }
        }
    }
}