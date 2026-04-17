using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;
using System.Security.Cryptography;
using System.Text;

namespace SwoopMarketplaceProjectFrontend.Pages.Messages
{
    public class ConversationModel : PageModel
    {
        private readonly MessageApi _msgApi;
        private readonly AuthSession _auth;
        private readonly UserApi _userApi;

        public ConversationModel(MessageApi msgApi, AuthSession auth, UserApi userApi)
        {
            _msgApi = msgApi;
            _auth = auth;
            _userApi = userApi;
        }

        [BindProperty(SupportsGet = true)] public long To { get; set; }
        [BindProperty(SupportsGet = true)] public long? Listing { get; set; }
        [BindProperty] public string NewMessage { get; set; } = "";

        // typed OtherUser so view can access ProfileImageUrl directly
        public MessageApi.OtherUserDto? OtherUser { get; set; }
        public List<MessageApi.MessageDto> Messages { get; set; } = new();

        // resolved current user info for the view (so AuthSession.GetUserId null won't break layout)
        public long? CurrentUserId { get; set; }
        public string? CurrentUserEmail { get; set; }
        public string? CurrentUserProfileImageUrl { get; set; }

        // listing information if conversation is about a specific listing
        public MessageApi.ListingDto? ListingInfo { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Messages/Conversation", new { to = To, listing = Listing }) });
            if (To <= 0) return BadRequest();
            await LoadConversationDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");
            if (To <= 0) return BadRequest();

            if (string.IsNullOrWhiteSpace(NewMessage))
            {
                await LoadConversationDataAsync();
                return Page();
            }

            try
            {
                await _msgApi.SendAsync(To, NewMessage.Trim(), Listing);
                NewMessage = "";
                return RedirectToPage(new { to = To, listing = Listing });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hiba történt az üzenet küldése közben: " + ex.Message;
                await LoadConversationDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteConversationAsync()
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            try
            {
                await _msgApi.DeleteConversationAsync(To, Listing);
            }
            catch { /* ignore, already gone */ }

            return RedirectToPage("/Messages/Index");
        }

        private async Task LoadConversationDataAsync()
        {
            // Resolve conversation
            var conv = await _msgApi.GetMessagesWithAsync(To, Listing);
            OtherUser = conv.OtherUser;
            Messages = conv.Messages;
            ListingInfo = conv.Listing;

            // Set current user email
            CurrentUserEmail = _auth.GetEmail();

            // Resolve current user id: prefer token claim, fallback to user lookup by email
            var idFromToken = _auth.GetUserId();
            if (idFromToken.HasValue)
            {
                CurrentUserId = idFromToken.Value;
            }
            else if (!string.IsNullOrWhiteSpace(CurrentUserEmail))
            {
                try
                {
                    var me = await _userApi.GetByEmailAsync(CurrentUserEmail);
                    if (me != null) CurrentUserId = me.Id;
                }
                catch { }
            }

            // get current user's profile image (if available) from UserApi
            if (CurrentUserId.HasValue)
            {
                try
                {
                    var me = await _userApi.GetByAzonAsync(CurrentUserId.Value);
                    if (me != null && !string.IsNullOrWhiteSpace(me.ProfileImageUrl))
                    {
                        CurrentUserProfileImageUrl = me.ProfileImageUrl.StartsWith("http")
                            ? me.ProfileImageUrl
                            : $"https://localhost:7000{(me.ProfileImageUrl.StartsWith("/") ? "" : "/")}{me.ProfileImageUrl}";
                    }
                }
                catch { }
            }
        }

        public string GetGravatarUrl(string? email, int size = 128)
        {
            if (string.IsNullOrWhiteSpace(email)) return "/images/avatar.png";
            using var md5 = MD5.Create();
            var hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant()))).Replace("-", "").ToLowerInvariant();
            return $"https://www.gravatar.com/avatar/{hash}?s={size}&d=identicon";
        }
    }
}