using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(SwoopContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // DTO for updates - keeps binding small and safe
        public class UserUpdateDto
        {
            public long? Id { get; set; }             // accept optional Id in payload and validate it
            public string? Email { get; set; }        // accept optional Email so frontend can send it
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
            var appUsers = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.ProfileImageUrl,
                    u.Bio,
                    u.CreatedAt
                })
                .ToListAsync();

            var result = new List<object>();
            foreach (var u in appUsers)
            {
                List<string> roles = new();
                if (!string.IsNullOrWhiteSpace(u.Email))
                {
                    var identityUser = await _userManager.FindByEmailAsync(u.Email);
                    if (identityUser != null)
                    {
                        roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
                    }
                }

                result.Add(new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.ProfileImageUrl,
                    u.Bio,
                    u.CreatedAt,
                    Roles = roles
                });
            }

            return Ok(result);
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

            if (user == null) return NotFound();

            List<string> roles = new();
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var identityUser = await _userManager.FindByEmailAsync(user.Email);
                if (identityUser != null)
                    roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
            }

            return Ok(new {
                user.Id,
                user.Username,
                user.Email,
                user.Phone,
                user.ProfileImageUrl,
                user.Bio,
                user.CreatedAt,
                Roles = roles
            });
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> PutUser(long id, [FromBody] UserUpdateDto dto)
        {
            if (dto is null)
                return BadRequest("Nincs beérkező adat..");

            // if payload contains an Id, ensure it matches route id
            if (dto.Id.HasValue && dto.Id.Value != id)
                return BadRequest("Id nem egyezik az útvonallal.");

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
                    return BadRequest("A felhasználónév már használatban van!");
            }

            // Validate uniqueness if phone changed (non-empty)
            if (!string.IsNullOrWhiteSpace(dto.Phone) &&
                !string.Equals(dto.Phone, existing.Phone, StringComparison.OrdinalIgnoreCase))
            {
                var phoneTaken = await _context.Users
                    .AnyAsync(u => u.Id != id && u.Phone == dto.Phone);
                if (phoneTaken)
                    return BadRequest("A telefonszám már használatban van!");
            }

            // Validate uniqueness if email changed (non-empty)
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !string.Equals(dto.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _context.Users
                    .AnyAsync(u => u.Id != id && u.Email.ToLower() == dto.Email.ToLower());
                if (emailTaken)
                    return BadRequest("Az email már használatban van!");
            }

            // Apply only allowed updates - do not overwrite PasswordHash, CreatedAt, or Id
            existing.Username = string.IsNullOrWhiteSpace(dto.Username) ? existing.Username : dto.Username;
            existing.Phone = dto.Phone ?? existing.Phone;
            existing.ProfileImageUrl = dto.ProfileImageUrl ?? existing.ProfileImageUrl;
            existing.Bio = dto.Bio ?? existing.Bio;

            // If frontend included Email and it passed uniqueness check, update it.
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                existing.Email = dto.Email;
            }

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

        // Request model used for form uploads (IFormFile must be a property here for Swagger)
        public class UploadProfileImageRequest
        {
            public IFormFile File { get; set; } = null!;
        }

        // POST: api/Users/{id}/upload-photo
        // Uploads profile photo, saves under wwwroot/images/profilepictures and updates user's ProfileImageUrl
        [HttpPost("{id}/upload-photo")]
        [Authorize(Roles = "Admin,User")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePhoto(long id, [FromForm] UploadProfileImageRequest request)
        {
            if (request?.File == null || request.File.Length == 0)
                return BadRequest("Nincs feltöltve fájl.");

            var file = request.File;

            var existing = await _context.Users.FindAsync(id);
            if (existing == null)
                return NotFound("Nem találtuk a felhasználót.");

            // resolve caller email from JWT claims
            var callerEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            if (string.IsNullOrWhiteSpace(callerEmail))
                return Forbid();

            // only owner or admin may update the photo
            if (!User.IsInRole("Admin") && !string.Equals(existing.Email, callerEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesDir = Path.Combine(wwwroot, "images", "profilepictures");
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{(string.IsNullOrEmpty(ext) ? "" : ext)}";
            var filePath = Path.Combine(imagesDir, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/images/profilepictures/{fileName}";

            // update user record
            existing.ProfileImageUrl = relativeUrl;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = relativeUrl });
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
