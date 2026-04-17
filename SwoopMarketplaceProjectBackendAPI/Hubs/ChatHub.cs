using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;
using System.Security.Claims;

namespace SwoopMarketplaceProjectBackendAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly SwoopContext _context;

        public ChatHub(SwoopContext context) => _context = context;

        // ── Segédmetódus: aktuális felhasználó lekérése ──
        private async Task<User?> ResolveCallerAsync()
        {
            var email = Context.User?.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email ||
                c.Type == "email" ||
                c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email
            )?.Value;

            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // ── Belépés a conversation szobába ──
        public async Task JoinConversation(long otherUserId, long? listingId)
        {
            var caller = await ResolveCallerAsync();
            if (caller == null) return;

            // Szoba neve: a két userId sorba rendezve + opcionális listingId
            var roomId = BuildRoomId(caller.Id, otherUserId, listingId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Olvasatlan üzenetek megjelölése
            var unread = await _context.Messages
                .Where(m => m.ToUserId == caller.Id && m.FromUserId == otherUserId && !m.IsRead
                    && (!listingId.HasValue || m.ListingId == listingId.Value))
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var m in unread) m.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        // ── Üzenet küldése ──
        public async Task SendMessage(long toUserId, string content, long? listingId)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var caller = await ResolveCallerAsync();
            if (caller == null) return;

            var msg = new Message
            {
                FromUserId = caller.Id,
                ToUserId = toUserId,
                ListingId = listingId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            var roomId = BuildRoomId(caller.Id, toUserId, listingId);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", new
            {
                msg.Id,
                msg.FromUserId,
                msg.ToUserId,
                msg.Content,
                msg.CreatedAt,
                msg.IsEdited,
                SenderName = caller.Username ?? caller.Email
            });
        }

        // ── Üzenet szerkesztése ──
        public async Task EditMessage(long messageId, string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent)) return;

            var caller = await ResolveCallerAsync();
            if (caller == null) return;

            var msg = await _context.Messages.FindAsync(messageId);
            if (msg == null || msg.FromUserId != caller.Id) return;

            msg.Content = newContent.Trim();
            msg.IsEdited = true;
            await _context.SaveChangesAsync();

            var otherUserId = msg.ToUserId;
            var roomId = BuildRoomId(caller.Id, otherUserId, msg.ListingId);
            await Clients.Group(roomId).SendAsync("MessageEdited", new
            {
                msg.Id,
                msg.Content
            });
        }

        // ── Üzenet törlése ──
        public async Task DeleteMessage(long messageId)
        {
            var caller = await ResolveCallerAsync();
            if (caller == null) return;

            var msg = await _context.Messages.FindAsync(messageId);
            if (msg == null || msg.FromUserId != caller.Id) return;

            var otherUserId = msg.ToUserId;
            var listingId = msg.ListingId;

            _context.Messages.Remove(msg);
            await _context.SaveChangesAsync();

            var roomId = BuildRoomId(caller.Id, otherUserId, listingId);
            await Clients.Group(roomId).SendAsync("MessageDeleted", messageId);
        }

        // ── Szoba azonosító: mindkét félnek ugyanaz ──
        private static string BuildRoomId(long a, long b, long? listingId)
        {
            var lo = Math.Min(a, b);
            var hi = Math.Max(a, b);
            return listingId.HasValue
                ? $"chat_{lo}_{hi}_l{listingId.Value}"
                : $"chat_{lo}_{hi}";
        }
    }
}