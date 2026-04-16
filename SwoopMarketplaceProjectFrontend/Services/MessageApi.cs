using System.Net.Http.Json;
using SwoopMarketplaceProjectFrontend.Services;
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
            return res ?? new List<ConversationDto>();
        }

        public async Task<ConversationMessagesDto> GetMessagesWithAsync(long otherUserId, long? listingId = null)
        {
            var client = _f.CreateClient("SwoopApi");
            var url = $"api/Messages/with/{otherUserId}";
            if (listingId.HasValue) url += $"?listingId={listingId.Value}";
            var res = await client.GetFromJsonAsync<ConversationMessagesDto>(url);

            // Normalize OtherUser.ProfileImageUrl to absolute if necessary (same approach as UserApi)
            if (res?.OtherUser != null && client.BaseAddress != null)
            {
                var raw = res.OtherUser.ProfileImageUrl;
                if (!string.IsNullOrWhiteSpace(raw) && !System.Uri.IsWellFormedUriString(raw, System.UriKind.Absolute))
                {
                    res.OtherUser.ProfileImageUrl = new System.Uri(client.BaseAddress, raw.StartsWith("/") ? raw : $"/{raw}").ToString();
                }
            }

            return res ?? new ConversationMessagesDto();
        }

        public async Task SendAsync(long toUserId, string content, long? listingId = null)
        {
            var client = _f.CreateClient("SwoopApi");
            var payload = new
            {
                ToUserId = toUserId,
                ListingId = listingId,
                Content = content
            };
            var r = await client.PostAsJsonAsync("api/Messages", payload);
            r.EnsureSuccessStatusCode();
        }

        // DTOs mirrored from backend
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
        }

        // Strongly-typed OtherUser DTO (matches backend anonymous object keys)
        public class OtherUserDto
        {
            public long Id { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? ProfileImageUrl { get; set; }
        }

        public class ConversationMessagesDto
        {
            public OtherUserDto? OtherUser { get; set; }
            public List<MessageDto> Messages { get; set; } = new();
        }
    }
}