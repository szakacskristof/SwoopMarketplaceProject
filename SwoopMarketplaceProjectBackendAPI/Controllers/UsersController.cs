using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SwoopContext _context;

        public UsersController(SwoopContext context)
        {
            _context = context;
        }

        // DTO for updates - keeps binding small and safe
        public class UserUpdateDto
        {
            public long Id { get; set; }

            public string Email{ get; set; }
            public string? Username { get; set; }
            public string? Phone { get; set; }
            public string? ProfileImageUrl { get; set; }
            public string? Bio { get; set; }
        }

        // GET: api/Users
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            // return lightweight user projection (avoid exposing PasswordHash)
            var users = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.Username,
                    u.Phone,
                    u.ProfileImageUrl,
                    u.Bio,
                    u.CreatedAt
                })
                .ToListAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(long id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.ProfileImageUrl,
                    u.Bio,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> PutUser(long id, [FromBody] UserUpdateDto dto)
        {
            if (dto is null)
                return BadRequest("No data supplied.");

            // load existing user from DB
            var existing = await _context.Users.FindAsync(id);
            if (existing == null)
                return NotFound();

            // resolve caller email from JWT claims
            var callerEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            if (string.IsNullOrWhiteSpace(callerEmail))
                return Forbid();

            // if caller is not admin, ensure ownership
            if (!User.IsInRole("Admin") && !string.Equals(existing.Email, callerEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Validate uniqueness if username changed
            if (!string.IsNullOrWhiteSpace(dto.Username) &&
                !string.Equals(dto.Username, existing.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameTaken = await _context.Users
                    .AnyAsync(u => u.Id != id && u.Username.ToLower() == dto.Username.ToLower());
                if (usernameTaken)
                    return BadRequest("Username already in use.");
            }

            // Validate uniqueness if phone changed (non-empty)
            if (!string.IsNullOrWhiteSpace(dto.Phone) &&
                !string.Equals(dto.Phone, existing.Phone, StringComparison.OrdinalIgnoreCase))
            {
                var phoneTaken = await _context.Users
                    .AnyAsync(u => u.Id != id && u.Phone == dto.Phone);
                if (phoneTaken)
                    return BadRequest("Phone number already in use.");
            }

            // Apply only allowed updates - do not overwrite PasswordHash, CreatedAt, Email or Id
            existing.Username = string.IsNullOrWhiteSpace(dto.Username) ? existing.Username : dto.Username;
            existing.Phone = dto.Phone ?? existing.Phone;
            existing.ProfileImageUrl = dto.ProfileImageUrl ?? existing.ProfileImageUrl;
            existing.Bio = dto.Bio ?? existing.Bio;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Unique constraint or other DB error
                return StatusCode(500, dbEx.GetBaseException()?.Message ?? dbEx.Message);
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
