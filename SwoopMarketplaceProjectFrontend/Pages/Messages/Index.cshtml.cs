using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwoopMarketplaceProjectFrontend.Services;
using System.Threading.Tasks;

namespace SwoopMarketplaceProjectFrontend.Pages.Messages
{
    public class IndexModel : PageModel
    {
        private readonly MessageApi _msgApi;
        private readonly AuthSession _auth;

        public IndexModel(MessageApi msgApi, AuthSession auth)
        {
            _msgApi = msgApi;
            _auth = auth;
        }

        public System.Collections.Generic.List<MessageApi.ConversationDto> Conversations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_auth.IsSignedIn) return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Messages/Index") });
            Conversations = await _msgApi.GetConversationsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteConversationAsync(long otherUserId, long? listingId)
        {
            if (!_auth.IsSignedIn)
                return RedirectToPage("/Account/Login");

            try
            {
                await _msgApi.DeleteConversationAsync(otherUserId, listingId);
            }
            catch { /* ignore */ }

            return RedirectToPage();
        }
    }
}