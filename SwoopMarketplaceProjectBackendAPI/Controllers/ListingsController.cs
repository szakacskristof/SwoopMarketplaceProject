using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly SwoopContext _context;

        public ListingsController(SwoopContext context)
        {
            _context = context;
        }

        // GET: api/Listings
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetListings()
        {
            // return lightweight projection to avoid serializing navigation properties
            var list = await _context.Listings
                .Select(l => new {
                    l.Id,
                    l.UserId,
                    l.CategoryId,
                    CategoryName = l.Category != null ? l.Category.Name : null,
                    l.Title,
                    l.Description,
                    l.Price,
                    l.Condition,
                    l.Status,
                    l.Location,
                    l.CreatedAt,
                    l.UpdatedAt,
                    ImageUrls = l.ListingImages
                        .OrderByDescending(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Listings/5
        [AllowAnonymous]
        [HttpGet("{id:long}")]
        public async Task<ActionResult<object>> GetListing(long id)
        {
            var listing = await _context.Listings
                .Where(l => l.Id == id)
                .Select(l => new {
                    l.Id,
                    l.UserId,
                    l.CategoryId,
                    CategoryName = l.Category != null ? l.Category.Name : null,
                    l.Title,
                    l.Description,
                    l.Price,
                    l.Condition,
                    l.Status,
                    l.Location,
                    l.CreatedAt,
                    l.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (listing == null)
            {
                return NotFound();
            }

            return Ok(listing);
        }

        // GET: api/Listings/bycategory/{category}
        [AllowAnonymous]
        [HttpGet("bycategory/{category}")]
        public async Task<ActionResult<IEnumerable<object>>> GetListingByCategory(string category)
        {
            var listings = await _context.Listings
                .Where(x => x.Category != null && x.Category.Name == category)
                .Select(l => new {
                    l.Id,
                    l.UserId,
                    l.CategoryId,
                    CategoryName = l.Category != null ? l.Category.Name : null,
                    l.Title,
                    l.Description,
                    l.Price,
                    l.Condition,
                    l.Status,
                    l.Location,
                    l.CreatedAt,
                    l.UpdatedAt
                })
                .ToListAsync();

            return Ok(listings);
        }

        // PUT: api/Listings/5
        // Only the owner (or Admin) can update their listing.
        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> PutListing(long id, Listing listing)
        {
            if (id != listing.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Listings
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existing is null)
                return NotFound();

            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // Allow Admins even if they don't have a row in the custom users table.
            if (!User.IsInRole("Admin"))
            {
                if (appUser is null)
                    return Forbid();
                if (appUser.Id != existing.UserId)
                    return Forbid();
            }

            existing.Title = listing.Title;
            existing.Description = listing.Description;
            existing.Price = listing.Price;
            existing.Condition = listing.Condition;
            existing.Status = listing.Status;
            existing.Location = listing.Location;
            existing.CategoryId = listing.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Listings
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<Listing>> PostListing(Listing listing)
        {
            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Forbid();
            }

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (appUser is null)
            {
                return Forbid();
            }

            listing.UserId = appUser.Id;
            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
        }

        // DELETE: api/Listings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteListing(long id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
            {
                return NotFound();
            }

            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // Allow Admins to delete even if they don't have a corresponding row in the app users table.
            if (!User.IsInRole("Admin"))
            {
                if (appUser is null)
                    return Forbid();
                if (appUser.Id != listing.UserId)
                    return Forbid();
            }

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ListingExists(long id)
        {
            return _context.Listings.Any(e => e.Id == id);
        }
    }
}
