using System.Net.Http.Json;
using SwoopMarketplaceProjectFrontend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public class MessageApi
    {
        private readonly IHttpClientFactory _f;
        public MessageApi(IHttpClientFactory f) => _f = f;

        public async Task<List<ConversationDto>> GetConversationsAsync()
        {
            var client = _f.CreateClient("SwoopApi");
            var res = await client.GetFromJsonAsync<List<ConversationDto>>("api/Messages/conversations");
            return res ?? new();
        }

        public async Task<ConversationMessagesDto> GetMessagesWithAsync(long otherUserId, long? listingId = null)
        {
            var client = _f.CreateClient("SwoopApi");
            var url = $"api/Messages/with/{otherUserId}";
            if (listingId.HasValue) url += $"?listingId={listingId.Value}";
            var res = await client.GetFromJsonAsync<ConversationMessagesDto>(url);

            if (res?.OtherUser != null && client.BaseAddress != null)
            {
                var raw = res.OtherUser.ProfileImageUrl;
                if (!string.IsNullOrWhiteSpace(raw) && !System.Uri.IsWellFormedUriString(raw, System.UriKind.Absolute))
                    res.OtherUser.ProfileImageUrl = new System.Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
            }

            return res ?? new();
        }

        public async Task SendAsync(long toUserId, string content, long? listingId = null)
        {
            var client = _f.CreateClient("SwoopApi");
            var r = await client.PostAsJsonAsync("api/Messages", new { ToUserId = toUserId, ListingId = listingId, Content = content });
            r.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Soft-deletes the conversation for the currently signed-in user only.
        /// The other participant still sees all messages.
        /// </summary>
        public async Task DeleteConversationAsync(long otherUserId, long? listingId = null)
        {
            var client = _f.CreateClient("SwoopApi");
            var url = $"api/Messages/conversation/{otherUserId}";
            if (listingId.HasValue) url += $"?listingId={listingId.Value}";
            var r = await client.DeleteAsync(url);
            r.EnsureSuccessStatusCode();
        }

        // ── DTOs ──
        public class ConversationDto
        {
            public long OtherUserId { get; set; }
            public string OtherUserName { get; set; } = "";
            public string? OtherUserEmail { get; set; }
            public string? OtherProfileImageUrl { get; set; }
            public string LastMessage { get; set; } = "";
            public System.DateTime LastAt { get; set; }
            public int UnreadCount { get; set; }
            public long? ListingId { get; set; }
            public string? ListingTitle { get; set; }
        }

        public class MessageDto
        {
            public long Id { get; set; }
            public long FromUserId { get; set; }
            public long ToUserId { get; set; }
            public long? ListingId { get; set; }
            public string Content { get; set; } = "";
            public System.DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public bool IsEdited { get; set; }
        }

        public class OtherUserDto
        {
            public long Id { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? ProfileImageUrl { get; set; }
        }

        public class ListingDto
        {
            public long Id { get; set; }
            public string Title { get; set; } = "";
        }

        public class ConversationMessagesDto
        {
            public OtherUserDto? OtherUser { get; set; }
            public List<MessageDto> Messages { get; set; } = new();
            public ListingDto? Listing { get; set; }
        }
    }
}