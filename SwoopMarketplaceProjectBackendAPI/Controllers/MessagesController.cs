using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly SwoopContext _context;

        public MessagesController(SwoopContext context)
        {
            _context = context;
        }

        // DTOs
        public class CreateMessageDto
        {
            public long ToUserId { get; set; }
            public long? ListingId { get; set; }
            public string Content { get; set; } = "";
        }

        public class MessageDto
        {
            public long Id { get; set; }
            public long FromUserId { get; set; }
            public long ToUserId { get; set; }
            public long? ListingId { get; set; }
            public string Content { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
        }

        public class ConversationDto
        {
            public long OtherUserId { get; set; }
            public string OtherUserName { get; set; } = "";
            public string? OtherUserEmail { get; set; }
            public string? OtherProfileImageUrl { get; set; }
            public string LastMessage { get; set; } = "";
            public DateTime LastAt { get; set; }
            public int UnreadCount { get; set; }
            public long? ListingId { get; set; }
            public string? ListingTitle { get; set; }
        }

        // Helper to resolve caller application user by email claim
        private async Task<User?> ResolveCallerAsync()
        {
            var email = User.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, StringComparison.OrdinalIgnoreCase)
            )?.Value;

            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // POST: api/Messages
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MessageDto>> PostMessage([FromBody] CreateMessageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Hibás adatok.");

            var caller = await ResolveCallerAsync();
            if (caller == null) return Forbid();

            var toUser = await _context.Users.FindAsync(dto.ToUserId);
            if (toUser == null) return NotFound("Nem találtuk a felhasználót.");

            var msg = new Message
            {
                FromUserId = caller.Id,
                ToUserId = dto.ToUserId,
                ListingId = dto.ListingId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            var result = new MessageDto
            {
                Id = msg.Id,
                FromUserId = msg.FromUserId,
                ToUserId = msg.ToUserId,
                ListingId = msg.ListingId,
                Content = msg.Content,
                CreatedAt = msg.CreatedAt,
                IsRead = msg.IsRead
            };

            return CreatedAtAction(nameof(GetMessagesWith), new { otherUserId = dto.ToUserId }, result);
        }

        // GET: api/Messages/conversations
        // returns latest conversation per other user for caller
        [HttpGet("conversations")]
        [Authorize]
        public async Task<ActionResult> GetConversations()
        {
            var caller = await ResolveCallerAsync();
            if (caller == null) return Forbid();
            var myId = caller.Id;

            // gather messages where caller is either sender or recipient
            var msgs = _context.Messages
                .Where(m => m.FromUserId == myId || m.ToUserId == myId);

            // group by conversation partner + optional listing link
            var grouped = await msgs
                .GroupBy(m => new { Partner = (m.FromUserId == myId ? m.ToUserId : m.FromUserId), m.ListingId })
                .Select(g => new
                {
                    OtherUserId = g.Key.Partner,
                    ListingId = g.Key.ListingId,
                    LastMessage = g.OrderByDescending(x => x.CreatedAt).FirstOrDefault(),
                    Unread = g.Count(x => x.ToUserId == myId && !x.IsRead)
                })
                .ToListAsync();

            var result = grouped.Select(g =>
            {
                var other = _context.Users.FirstOrDefault(u => u.Id == g.OtherUserId);
                var listing = g.ListingId.HasValue ? _context.Listings.FirstOrDefault(l => l.Id == g.ListingId.Value) : null;
                
                return new ConversationDto
                {
                    OtherUserId = g.OtherUserId,
                    OtherUserName = other?.Username ?? other?.Email ?? ("user:" + g.OtherUserId),
                    OtherUserEmail = other?.Email,
                    OtherProfileImageUrl = other?.ProfileImageUrl,
                    LastMessage = g.LastMessage?.Content ?? "",
                    LastAt = g.LastMessage?.CreatedAt ?? DateTime.MinValue,
                    UnreadCount = g.Unread,
                    ListingId = g.ListingId,
                    ListingTitle = listing?.Title
                };
            })
            .OrderByDescending(c => c.LastAt)
            .ToList();

            return Ok(result);
        }

        // GET: api/Messages/with/{otherUserId}?listingId=...
        [HttpGet("with/{otherUserId}")]
        [Authorize]
        public async Task<ActionResult> GetMessagesWith(long otherUserId, [FromQuery] long? listingId = null)
        {
            var caller = await ResolveCallerAsync();
            if (caller == null) return Forbid();
            var myId = caller.Id;

            // Validate other exists
            var other = await _context.Users.FindAsync(otherUserId);
            if (other == null) return NotFound("Nem találtuk a másik felhasználót.");

            try
            {
                var query = _context.Messages
                    .Where(m =>
                        (m.FromUserId == myId && m.ToUserId == otherUserId) ||
                        (m.FromUserId == otherUserId && m.ToUserId == myId));

                if (listingId.HasValue)
                    query = query.Where(m => m.ListingId == listingId.Value);

                var messages = await query
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        FromUserId = m.FromUserId,
                        ToUserId = m.ToUserId,
                        ListingId = m.ListingId,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        IsRead = m.IsRead
                    })
                    .ToListAsync();

                // Mark messages received by caller as read
                var unread = await _context.Messages
                    .Where(m => m.ToUserId == myId && m.FromUserId == otherUserId && !m.IsRead && (!listingId.HasValue || m.ListingId == listingId.Value))
                    .ToListAsync();

                if (unread.Any())
                {
                    foreach (var m in unread) m.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                // Get listing title if listingId is provided
                var listing = listingId.HasValue ? await _context.Listings.FindAsync(listingId.Value) : null;

                return Ok(new
                {
                    OtherUser = new
                    {
                        other.Id,
                        other.Username,
                        other.Email,
                        other.ProfileImageUrl
                    },
                    Messages = messages,
                    Listing = listing != null ? new { listing.Id, listing.Title } : null
                });
            }
            catch (MySqlConnector.MySqlException ex)
            {
                // Give a clear diagnostic message in development/logging environments,
                // but do not leak sensitive info in production.
                // This typically happens when the underlying 'messages' table does not exist.
                return Problem(detail: "Database error while querying messages: " + ex.Message, statusCode: 500);
            }
            catch (Exception ex)
            {
                return Problem(detail: "Unexpected error while querying messages: " + ex.Message, statusCode: 500);
            }
        }
    }
}